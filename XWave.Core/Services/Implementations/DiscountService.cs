using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using XWave.Core.Data;
using XWave.Core.DTOs.Management;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;
using XWave.Core.Utils;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Implementations;

internal class DiscountService : ServiceBase, IDiscountService
{
    private readonly IActivityService _activityService;
    private readonly IBackgroundJobService _backgroundJobService;
    private readonly ILogger<DiscountService> _logger;
    private readonly IAuthorizationService _authorizationService;

    private readonly Error _unauthorizedOperationError = new()
    {
        ErrorCode = ErrorCode.InvalidUserRequest,
        Message = "Only managers are authorized to modify/ assign Discounts",
    };

    public DiscountService(
        XWaveDbContext dbContext,
        IActivityService activityService,
        IAuthorizationService authorizationService,
        ILogger<DiscountService> logger,
        IBackgroundJobService backgroundJobService) : base(dbContext)
    {
        _logger = logger;
        _activityService = activityService;
        _authorizationService = authorizationService;
        _backgroundJobService = backgroundJobService;
    }

    public async Task<ServiceResult<int>> CreateDiscountAsync(
        string managerId,
        DiscountViewModel discountViewModel)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult<int>.Failure(_unauthorizedOperationError);
        }

        var newDiscount = new Discount();
        var entry = DbContext.Add(newDiscount);
        entry.CurrentValues.SetValues(discountViewModel);
        await DbContext.SaveChangesAsync();
        await _activityService.LogActivityAsync<Discount>(
            managerId,
            OperationType.Create,
            $"created discount with ID {newDiscount.Id}.");

        return ServiceResult<int>.Success(newDiscount.Id);
    }

    public async Task<IEnumerable<Product>> FindProductsWithDiscountIdAsync(int discountId)
    {
        return await DbContext.Product
            .Where(p => p.DiscountId == discountId)
            .ToListAsync();
    }

    public async Task<ServiceResult> RemoveDiscountAsync(string managerId, int discountId)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var discount = await DbContext.Discount.FindAsync(discountId);
        if (discount is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Discount with ID {discountId} was not found.",
            });
        }

        var percentage = discount.Percentage;
        await using var transaction = await DbContext.Database.BeginTransactionAsync();
        try
        {
            // start tracking items to avoid FK constraint errors because Delete.ClientSetNull actually does NOT work
            await DbContext.Product.Where(d => d.DiscountId == discountId).LoadAsync();
            DbContext.Discount.Remove(discount);
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Delete,
                $"removed a {percentage} discount with ID {discountId}.");

            await transaction.CommitAsync();
            return ServiceResult.Success();
        }
        catch (Exception exception)
        {
            await transaction.RollbackAsync();
            _logger.LogError($"Failed to remove discount with ID {discountId}");
            _logger.LogError(exception.Message);
            return ServiceResult.DefaultFailure();
        }
    }

    public Task<IEnumerable<DetailedDiscountDto>> FindAllDiscountsAsync()
    {
        var discounts = DbContext.Discount
            .AsEnumerable()
            .Select(d => DiscountDtoMapper.MapDetailedDiscountDto(d))
            .OrderBy(d => d.IsActive)
            .AsEnumerable();

        return Task.FromResult(discounts);
    }

    public async Task<DetailedDiscountDto?> FindDiscountByIdAsync(int id)
    {
        var discount = await DbContext.Discount.FindAsync(id);
        return discount is null ? null : DiscountDtoMapper.MapDetailedDiscountDto(discount);
    }

    public async Task<ServiceResult> UpdateDiscountAsync(
        string managerId, 
        int discountId,
        DiscountViewModel updatedDiscountViewModel)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var discount = await DbContext.Discount.FindAsync(discountId);
        if (discount is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Discount with ID {discountId} was not found.",
            });
        }

        var entry = DbContext.Discount.Update(discount);
        entry.CurrentValues.SetValues(updatedDiscountViewModel);
        await DbContext.SaveChangesAsync();
        await _activityService.LogActivityAsync<Discount>(
            managerId,
            OperationType.Delete,
            $"removed discount with ID {discountId}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> ApplyDiscountToProducts(string managerId, int discountId,
        IEnumerable<int> productIds)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var discount = await DbContext.Discount.FindAsync(discountId);
        if (discount is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Discount with ID {discountId} was not found.",
            });
        }

        var appliedProducts = await DbContext.Product.Where(x => productIds.Contains(x.Id)).ToListAsync();
        var missingProductIds = productIds.Except(appliedProducts.Select(x => x.Id));

        if (missingProductIds.Any())
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Products with IDs {string.Join(", ", missingProductIds)} not found"
            });
        }

        DbContext.Product.UpdateRange(appliedProducts.Select(x =>
        {
            x.DiscountId = discountId;
            return x;
        }));

        await DbContext.SaveChangesAsync();
        await _backgroundJobService.AddBackgroundJobAsync(
            () => RemoveDiscountFromProducts(appliedProducts.Select(x => x.Id)),
            new DateTimeOffset(discount.EndDate));
        await _activityService.LogActivityAsync<Discount>(
            managerId,
            OperationType.Modify,
            $"applied discount with ID {discountId} to the following products with IDs: {string.Join(", ", appliedProducts.Select(x => x.Id))}.");

        return ServiceResult.Success();
    }

    public async Task<ServiceResult> RemoveDiscountFromProductsAsync(string managerId, int discountId,
        IEnumerable<int> productIds)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var discount = await DbContext.Discount
            .SingleOrDefaultAsync(d => d.Id == discountId);

        if (discount is null)
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityNotFound,
                Message = $"Discount with ID {discountId} was not found.",
            });
        }

        var appliedProducts = await DbContext.Product
            .Where(x => productIds.Contains(x.Id))
            .ToArrayAsync();

        var productsWithoutDiscount = appliedProducts
            .Where(p => p.DiscountId is null)
            .ToArray();

        if (productsWithoutDiscount.Any())
        {
            return ServiceResult.Failure(new Error
            {
                ErrorCode = ErrorCode.EntityInconsistentStates,
                Message = $"Discount with ID {discountId} is not applied to the following products: {string.Join(", ", productsWithoutDiscount.Select(p => p.Name))}.",
            });
        }

        DbContext.Product.UpdateRange(appliedProducts.Select(p =>
        {
            p.Discount = null;
            return p;
        }));

        await DbContext.SaveChangesAsync();
        await _activityService.LogActivityAsync<Discount>(
            managerId,
            OperationType.Modify,
            $"manually removed discount with ID {discountId} from products with the following IDs: {string.Join(", ", appliedProducts.Select(p => p.Id))}.");

        return ServiceResult.Success();
    }

    public async Task RemoveDiscountFromProducts(IEnumerable<int> appliedProductIds)
    {
        var products = await DbContext.Product
            .IgnoreAutoIncludes()
            .Where(x => appliedProductIds.Contains(x.Id))
            .ToListAsync();

        DbContext.Product.UpdateRange(products.Select(x =>
        {
            x.Discount = null;
            return x;
        }));

        await DbContext.SaveChangesAsync();
    }
}