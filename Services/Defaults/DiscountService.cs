using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.DTOs.Management;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.Utils;
using XWave.ViewModels.Management;

namespace XWave.Services.Defaults
{
    public class DiscountService : ServiceBase, IDiscountService
    {
        private readonly IActivityService _activityService;
        private readonly ILogger<DiscountService> _logger;

        public DiscountService(
            XWaveDbContext dbContext,
            IActivityService activityService,
            ILogger<DiscountService> logger) : base(dbContext)
        {
            _logger = logger;
            _activityService = activityService;
        }

        public async Task<(ServiceResult, int? DiscountId)> CreateDiscountAsync(string managerId, DiscountViewModel discountViewModel)
        {
            var newDiscount = new Discount();
            var entry = DbContext.Add(newDiscount);
            entry.CurrentValues.SetValues(discountViewModel);
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Create,
                $"created discount with ID {newDiscount.Id}.");

            return (ServiceResult.Success(), newDiscount.Id);
        }

        public async Task<IEnumerable<Product>> FindProductsWithDiscountIdAsync(int discountId)
        {
            return await DbContext.Product
                .Where(p => p.DiscountId == discountId)
                .ToListAsync();
        }

        public async Task<ServiceResult> RemoveDiscountAsync(string managerId, int id)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {id} not found.");
            }

            var percentage = discount.Percentage;
            await using var transaction = await DbContext.Database.BeginTransactionAsync();
            try
            {
                // start tracking items to avoid FK constraint errors because Delete.ClientSetNull actually does NOT work
                await DbContext.Product.Where(d => d.DiscountId == id).LoadAsync();
                DbContext.Discount.Remove(discount);
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Discount>(
                    managerId,
                    OperationType.Delete,
                    $"removed a {percentage} discount with ID {id}.");

                await transaction.CommitAsync();
                return ServiceResult.Success();
            }
            catch (Exception exception)
            {
                await transaction.RollbackAsync();
                _logger.LogError(exception.Message);
                _logger.LogError(exception.StackTrace);
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<IEnumerable<DetailedDiscountDto>> FindAllDiscountsAsync()
        {
            return await DbContext.Discount
                .Select(d => DiscountDtoMapper.MapDetailedDiscountDto(d))
                .OrderBy(d => d.IsActive)
                .ToListAsync();
        }

        public async Task<DetailedDiscountDto?> FindDiscountByIdAsync(int id)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            return discount == null ? null : DiscountDtoMapper.MapDetailedDiscountDto(discount);
        }

        public async Task<ServiceResult> UpdateDiscountAsync(string managerId, int id, DiscountViewModel updatedDiscountViewModel)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            if (discount == null)
            {
                return ServiceResult.Failure("Not found");
            }

            var entry = DbContext.Discount.Update(discount);
            entry.CurrentValues.SetValues(updatedDiscountViewModel);
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Delete,
                $"removed discount with ID {id}.");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ApplyDiscountToProducts(string managerId, int discountId, IEnumerable<int> productIds)
        {
            var discount = await DbContext.Discount.FindAsync(discountId);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {discountId} was not found.");
            }

            var appliedProducts = await DbContext.Product.Where(x => productIds.Contains(x.Id)).ToListAsync();
            DbContext.Product.UpdateRange(appliedProducts.Select(x => 
            { 
                x.DiscountId = discountId;
                return x;
            }));
            
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Modify,
                $"applied discount with ID {discountId} to the following products with IDs: {string.Join(", ", appliedProducts.Select(x => x.Id))}.");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RemoveDiscountFromProductsAsync(string managerId, int discountId, IEnumerable<int> productIds)
        {
            var discount = await DbContext.Discount
                .SingleOrDefaultAsync(d => d.Id == discountId);

            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {discountId} was not found.");
            }

            var appliedProducts = await DbContext.Product
                .Where(x => productIds.Contains(x.Id))
                .ToArrayAsync();
            
            var productsWithoutDiscount = appliedProducts
                .Where(p => p.DiscountId == null)
                .ToArray();
            
            if (productsWithoutDiscount.Any())
            {
                return ServiceResult.Failure($"Discount with ID {discountId} is not applied to product with the following IDs: {string.Join(", ", productsWithoutDiscount.Select(p => p.Id))}.");
            }

            DbContext.Product.UpdateRange(appliedProducts.Select(p => { p.Discount = null; return p; }));
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Modify,
                $"removed discount with ID {discountId} from products with the following IDs: {string.Join(", ", appliedProducts.Select(p => p.Id)) }.");

            return ServiceResult.Success();
        }
    }
}