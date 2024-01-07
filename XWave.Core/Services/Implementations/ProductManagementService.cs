using Hangfire;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Implementations;

internal class ProductManagementService : ServiceBase, IProductManagementService
{
    private readonly IStaffActivityLogger _activityService;
    private readonly IBackgroundJobClient _backgroundJobClient;
    private readonly ILogger<ProductManagementService> _logger;
    private readonly ProductDtoMapper _productDtoMapper;
    private readonly IRoleAuthorizer _roleAuthorizer;
    private readonly Error _unauthorizedError = new()
    {
        Code = ErrorCode.AuthorizationError,
        Message = "Only staff are authorized to modify products"
    };

    public ProductManagementService(
        XWaveDbContext dbContext,
        IStaffActivityLogger activityService,
        IBackgroundJobClient backgroundJobService,
        ProductDtoMapper productHelper,
        ILogger<ProductManagementService> logger,
        IRoleAuthorizer roleAuthorizer) : base(dbContext)
    {
        _productDtoMapper = productHelper;
        _activityService = activityService;
        _roleAuthorizer = roleAuthorizer;
        _backgroundJobClient = backgroundJobService;
        _logger = logger;
    }

    public async Task<ServiceResult<int>> AddProductAsync(string staffId,
        CreateProductViewModel productViewModel)
    {
        if (!await IsStaffIdValid(staffId))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        try
        {
            _logger.LogInformation(
                $"User with ID {staffId} is attempting to add product named {productViewModel.Name}");

            if (!await DbContext.Category.AnyAsync(c => c.Id == productViewModel.CategoryId))
            {
                return ServiceResult<int>.Failure(new Error
                {
                    Code = ErrorCode.InvalidState,
                    Message = "Category not found",
                });
            }

            var newProduct = new Product();
            DbContext.Product
                .Add(newProduct)
                .CurrentValues
                .SetValues(productViewModel);
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Product>(
                staffId,
                OperationType.Create,
                $"added product named {newProduct.Name} and priced ${newProduct.Price}");

            return ServiceResult<int>.Success(newProduct.Id);
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to create product: {exception.Message}.");
            return ServiceResult<int>.UnknownFailure();
        }
    }

    public async Task<ServiceResult> DeleteProductAsync(int productId, string managerId)
    {
        if (!await _roleAuthorizer.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        var product = await DbContext.Product.FindAsync(productId);
        if (product is null)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = "Product not found",
            });
        }

        DbContext.Product.Update(product);
        product.SoftDelete();
        await DbContext.SaveChangesAsync();
        await _activityService.LogActivityAsync<Product>(
            managerId,
            OperationType.Modify,
            $"deleted product named {product.Name}, ID = {product.Id} at {product.DeleteDate}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult<IReadOnlyCollection<DetailedProductDto>>> FindAllProductsForStaff(
        bool includeDiscontinuedProducts,
        string staffId)
    {
        if (!await IsStaffIdValid(staffId))
        {
            return ServiceResult<IReadOnlyCollection<DetailedProductDto>>.Failure(_unauthorizedError);
        }

        var products = await DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .Where(p => includeDiscontinuedProducts || !p.IsDiscontinued)
            .ToListAsync();

        var productDtos = products
            .Select(_productDtoMapper.MapDetailedProductDto)
            .ToList();

        return ServiceResult<IReadOnlyCollection<DetailedProductDto>>.Success(productDtos.AsIReadonlyCollection());
    }

    public async Task<ServiceResult<DetailedProductDto>> FindProductByIdForStaff(int id, string staffId)
    {
        if (!await IsStaffIdValid(staffId))
        {
            return ServiceResult<DetailedProductDto>.Failure(new Error
            {
                Code = ErrorCode.AuthorizationError
            });
        }

        var product = await DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null)
        {
            return ServiceResult<DetailedProductDto>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Product with ID {id} not found.",
            });
        }

        var productDto = _productDtoMapper.MapDetailedProductDto(product);

        return ServiceResult<DetailedProductDto>.Success(productDto);
    }

    public async Task<ServiceResult> UpdateProductAsync(
        string staffId,
        int productId,
        UpdateProductViewModel updatedProductViewModel)
    {
        if (!await IsStaffIdValid(staffId))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        _logger.LogInformation($"User with ID {staffId} is attempting to update product ID {productId}");
        var product = await DbContext.Product.FindAsync(productId);
        if (product is null)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Product with ID {productId} not found.",
            });
        }

        if (product.IsDiscontinued)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.ConflictingState,
                Message = $"Product with ID {productId} is discontinued",
            });
        }

        DbContext.Product
            .Update(product)
            .CurrentValues
            .SetValues(updatedProductViewModel);
        await DbContext.SaveChangesAsync();
        await _activityService.LogActivityAsync<Product>(
            staffId,
            OperationType.Modify,
            $"updated general information of product named {product.Name}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateStockAsync(string staffId, int productId, uint updatedStock)
    {
        if (!await IsStaffIdValid(staffId))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        var product = await DbContext.Product.FindAsync(productId);
        if (product is null)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = "Product not found.",
            });
        }

        var quantityBeforeRestock = product.Quantity;
        DbContext.Product.Update(product);
        product.Quantity = updatedStock;
        product.LatestRestock = DateTime.Now;
        await DbContext.SaveChangesAsync();
        await _activityService.LogActivityAsync<Product>(
            staffId,
            OperationType.Modify,
            $"updated stock of product named {product.Name} (from {quantityBeforeRestock} to {updatedStock}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> UpdateProductPriceAsync(
        string staffId,
        int productId,
        UpdateProductPriceViewModel viewModel)
    {
        if (!await IsStaffIdValid(staffId))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        var product = await DbContext.Product.FirstOrDefaultAsync(x => x.Id == productId);
        if (product is null)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = "Product not found.",
            });
        }

        if (viewModel.Schedule is null)
        {
            var formerPrice = product.Price;
            DbContext.Product.Update(product);
            product.Price = viewModel.UpdatedPrice;
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Product>(
                staffId,
                OperationType.Modify,
                $"updated price of product named {product.Name} (from {formerPrice} to {viewModel.UpdatedPrice}.");

            return ServiceResult.Success();
        }

        return await ScheduleProductPriceUpdate(staffId, productId, viewModel);
    }

    public async Task<ServiceResult> DiscontinueProductAsync(
        string managerId,
        IEnumerable<int> productIds,
        DateTime updateSchedule)
    {
        if (!await _roleAuthorizer.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        var productsToDiscontinue = await DbContext.Product
            .Where(product => productIds.Contains(product.Id))
            .ToArrayAsync();

        var missingProducts = productIds
            .Except(productsToDiscontinue.Select(p => p.Id))
            .ToArray();

        if (missingProducts.Any())
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Products with the following IDs not found: {string.Join(", ", missingProducts)}.",
            });
        }

        var discontinuedProducts = productsToDiscontinue
            .Where(p => p.IsDiscontinued)
            .Select(p => p.Id)
            .ToArray();

        if (discontinuedProducts.Any())
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.InvalidState,
                Message = $"Products with the following IDs already discontinued: {string.Join(", ", discontinuedProducts)}.",
            });
        }

        if (updateSchedule.IsBetween(DateTime.Now, DateTime.Now.AddDays(7)))
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled sale discontinuation date must be at least 1 week in the future."
            });
        }

        _backgroundJobClient.Schedule(
            () => UpdateProductSaleStatusByScheduleAsync(productIds, false, updateSchedule),
            new DateTimeOffset(updateSchedule));

        await _activityService.LogActivityAsync<Product>(
            managerId,
            OperationType.Modify,
            $"discontinued sale of product with IDs {string.Join(", ", productIds)}, effective at {updateSchedule:d MMMM yyyy}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RestartProductSaleAsync(string managerId, int productId, DateTime updateSchedule)
    {
        if (!await _roleAuthorizer.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult<int>.Failure(_unauthorizedError);
        }

        if (!await DbContext.Product.AnyAsync(p => p.Id == productId))
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = "Product not found.",
            });
        }

        if (updateSchedule.IsBetween(DateTime.Now, DateTime.Now.AddDays(7)))
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled sale restart date must be at least 1 week in the future."
            });
        }

        _backgroundJobClient.Schedule(
            () => UpdateProductSaleStatusByScheduleAsync(productId, false, updateSchedule),
            new DateTimeOffset(updateSchedule));

        await _activityService.LogActivityAsync<Product>(
            managerId,
            OperationType.Modify,
            $"restarted sale of product ID {productId}, effective {updateSchedule:d MMMM yyyy}.");

        return ServiceResult.Success();
    }

    public async Task UpdateProductPriceByScheduleAsync(string staffId, int productId, uint updatedPrice)
    {
        var product = await DbContext.Product.FindAsync(productId);
        if (product is not null)
        {
            DbContext.Product.Update(product);
            product.Price = updatedPrice;
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Product>(
                staffId,
                OperationType.Modify,
                $"carried out a scheduled change in the price of product ID = {productId}. The new price is {updatedPrice}.");
        }
    }

    public async Task UpdateProductSaleStatusByScheduleAsync(int productId, bool isDiscontinued,
        DateTime updateSchedule)
    {
        var product = await DbContext.Product.FindAsync(productId);
        if (product is not null)
        {
            product.IsDiscontinued = isDiscontinued;
            product.DiscontinuationDate = isDiscontinued ? updateSchedule : null;
            await DbContext.SaveChangesAsync();
        }
    }

    public async Task UpdateProductSaleStatusByScheduleAsync(IEnumerable<int> productIds, bool isDiscontinued,
        DateTime updateSchedule)
    {
        var productsToUpdate = await DbContext.Product
            .Where(x => productIds.Contains(x.Id))
            .ToListAsync();

        DbContext.Product.UpdateRange(productsToUpdate.Select(x =>
        {
            x.IsDiscontinued = isDiscontinued;
            x.DiscontinuationDate = isDiscontinued ? updateSchedule : null;
            return x;
        }));

        await DbContext.SaveChangesAsync();
    }

    private async Task<ServiceResult> ScheduleProductPriceUpdate(
        string staffId,
        int productId,
        UpdateProductPriceViewModel viewModel)
    {
        if (viewModel.Schedule!.Value.IsBetween(DateTime.Now, DateTime.Now.AddDays(7)))
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled price change date must be at least 1 week in the future."
            });
        }

        _backgroundJobClient.Schedule(
            () => UpdateProductPriceByScheduleAsync(staffId, productId, viewModel.UpdatedPrice),
            new DateTimeOffset(viewModel.Schedule!.Value));

        await _activityService.LogActivityAsync<Product>(
            staffId,
            OperationType.Modify,
            $"scheduled a price update (to ${viewModel.UpdatedPrice}) for product ID {productId} at {viewModel.Schedule}.");

        return ServiceResult.Success();
    }

    private async Task<bool> IsStaffIdValid(string userId)
    {
        return await _roleAuthorizer.IsUserInRoles(userId, RoleNames.InternalPersonnelRoles);
    }
}
