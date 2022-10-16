using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Implementations;

internal class ProductService : ServiceBase, IProductService
{
    private readonly IActivityService _activityService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<ProductService> _logger;
    private readonly ProductDtoMapper _productDtoMapper;
    private readonly IAuthorizationService _authorizationService;

    private readonly string[] staffRoles = new[] { RoleNames.Staff, RoleNames.Manager };

    private readonly Error _unauthorizedError = new()
    {
        Code = ErrorCode.AuthorizationError,
        Message = "Only staff are authorized to modify products"
    };

    public ProductService(
        XWaveDbContext dbContext,
        IActivityService activityService,
        IBackgroundJobService backgroundJobService,
        ProductDtoMapper productHelper,
        ILogger<ProductService> logger,
        IAuthorizationService authorizationService) : base(dbContext)
    {
        _productDtoMapper = productHelper;
        _activityService = activityService;
        _authorizationService = authorizationService;
        _backgroundJobService = backgroundJobService;
        _logger = logger;
    }

    public async Task<ServiceResult<int>> AddProductAsync(string staffId,
        ProductViewModel productViewModel)
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
        if (!await _authorizationService.IsUserInRole(managerId, RoleNames.Manager))
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

        try
        {
            DbContext.Product.Update(product);
            product.SoftDelete();
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Product>(
                managerId,
                OperationType.Modify,
                $"deleted product named {product.Name}, ID = {product.Id} at {product.DeleteDate}.");

            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to delete product: {exception.Message}.");
            return ServiceResult.UnknownFailure();
        }
    }

    public Task<IReadOnlyCollection<ProductDto>> FindAllProductsForCustomers()
    {
        var productDtos = DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .Where(p => !p.IsDiscontinued)
            .AsEnumerable()
            .Select(p => p.Discount is null
                ? _productDtoMapper.MapCustomerProductDto(p)
                : _productDtoMapper.MapCustomerProductDto(p, CalculatePriceAfterDiscount(p)))
            .ToList();

        return Task.FromResult(productDtos.AsIReadonlyCollection());
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

    public async Task<ProductDto?> FindProductByIdForCustomers(int id)
    {
        var product = await DbContext.Product
            .AsNoTracking()
            .Include(p => p.Discount)
            .Include(p => p.Category)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (product is null) return null;

        return product.Discount is null
            ? _productDtoMapper.MapCustomerProductDto(product)
            : _productDtoMapper.MapCustomerProductDto(product, CalculatePriceAfterDiscount(product));
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
        ProductViewModel updatedProductViewModel)
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
                Message = "Product not found.",
            });
        }

        if (product.IsDiscontinued || product.IsDeleted)
        {
            return ServiceResult<int>.Failure(new Error
            {
                Code = ErrorCode.ConflictingState,
                Message = "Product removed or discontinued.",
            });
        }

        try
        {
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
        catch (Exception exception)
        {
            _logger.LogError($"Failed to update product with ID {productId}");
            _logger.LogDebug(exception, exception.Message);
            return ServiceResult.UnknownFailure();
        }
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

        try
        {
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
        catch (Exception exception)
        {
            _logger.LogCritical($"Failed to update stock of product with ID {product.Id}.");
            _logger.LogDebug(exception, exception.Message);
            return ServiceResult.UnknownFailure();
        }
    }

    public async Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice)
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

        try
        {
            var formerPrice = product.Price;
            DbContext.Product.Update(product);
            product.Price = updatedPrice;
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Product>(
                staffId,
                OperationType.Modify,
                $"updated price of product named {product.Name} (from {formerPrice} to {updatedPrice}.");

            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            _logger.LogError($"Failed to update general information of product with ID {productId}.");
            _logger.LogDebug(exception, exception.Message);
            return ServiceResult.UnknownFailure();
        }
    }

    public async Task<ServiceResult> UpdateProductPriceAsync(
        string staffId,
        int productId,
        uint updatedPrice,
        DateTime updateSchedule)
    {
        if (!await IsStaffIdValid(staffId))
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

        var now = DateTime.Now;
        if (updateSchedule < now || updateSchedule < now.AddDays(7))
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled price change date must be at least 1 week in the future."
            });
        }

        _backgroundJobService
            .AddBackgroundJobAsync(
                () => ScheduledUpdateProductPrice(staffId, productId, updatedPrice),
                new DateTimeOffset(updateSchedule))
            .Wait();

        await _activityService.LogActivityAsync<Product>(
            staffId,
            OperationType.Modify,
            $"scheduled a price update (to ${updatedPrice}) for product ID {productId} at {updateSchedule}.");

        return ServiceResult.Success();
    }

    /// <inheritdoc />
    /// <remarks>
    ///     This method will return a failed result if one of the passed product IDs belongs to no product or if one of the
    ///     products is already discontinued. However, it does not check for products that are already scheduled for
    ///     discontinuation.
    /// </remarks>
    public async Task<ServiceResult> DiscontinueProductAsync(
        string managerId,
        int[] productIds,
        DateTime updateSchedule)
    {
        if (!await _authorizationService.IsUserInRole(managerId, RoleNames.Manager))
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

        var now = DateTime.Now;
        if (updateSchedule < now || updateSchedule < now.AddDays(7))
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled sale discontinuation date must be at least 1 week in the future."
            });
        }

        _backgroundJobService
            .AddBackgroundJobAsync(
                () => UpdateProductSaleStatusByScheduleAsync(productIds, false, updateSchedule),
                new DateTimeOffset(updateSchedule))
            .Wait();

        await _activityService.LogActivityAsync<Product>(
            managerId,
            OperationType.Modify,
            $"discontinued sale of product with IDs {string.Join(", ", productIds)}, effective at {updateSchedule:d MMMM yyyy}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RestartProductSaleAsync(string managerId, int productId, DateTime updateSchedule)
    {
        if (!await _authorizationService.IsUserInRole(managerId, RoleNames.Manager))
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

        var now = DateTime.Now;
        if (updateSchedule < now || updateSchedule < now.AddDays(7))
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.InvalidArgument,
                Message = "Scheduled sale restart date must be at least 1 week in the future."
            });
        }

        _backgroundJobService
            .AddBackgroundJobAsync(
                () => UpdateProductSaleStatusByScheduleAsync(productId, false, updateSchedule),
                new DateTimeOffset(updateSchedule))
            .Wait();

        await _activityService.LogActivityAsync<Product>(
            managerId,
            OperationType.Modify,
            $"restarted sale of product ID {productId}, effective {updateSchedule:d MMMM yyyy}.");

        return ServiceResult.Success();
    }

    public decimal CalculatePriceAfterDiscount(Product product)
    {
        if (product.Discount is null) 
        {
            throw new InvalidOperationException($"Product ID {product.Id} does not have any discount");
        }
        
        return product.Price - product.Price * product.Discount.Percentage / 100;
    }

    public async Task ScheduledUpdateProductPrice(string staffId, int productId, uint updatedPrice)
    {
        var product = await DbContext.Product.FindAsync(productId);
        if (product is not null)
        {
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

    public async Task UpdateProductSaleStatusByScheduleAsync(int[] productIds, bool isDiscontinued,
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

    private async Task<bool> IsStaffIdValid(string userId)
    {
        return await _authorizationService.IsUserInRoles(userId, RoleNames.InternalPersonnelRoles);
    }
}