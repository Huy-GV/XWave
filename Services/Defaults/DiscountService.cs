using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Defaults
{
    public class DiscountService : ServiceBase, IDiscountService
    {
        private readonly IActivityService _staffActivityService;
        private readonly UserManager<ApplicationUser> _userManager;

        public DiscountService(
            XWaveDbContext dbContext,
            IActivityService staffActivityService,
            UserManager<ApplicationUser> userManager) : base(dbContext)
        {
            _staffActivityService = staffActivityService;
            _userManager = userManager;
        }

        public async Task<(ServiceResult, int? DiscountId)> CreateDiscountAsync(string managerId, DiscountViewModel discountViewModel)
        {
            var newDiscount = new Discount() { ManagerId = managerId };
            var entry = DbContext.Add(newDiscount);
            entry.CurrentValues.SetValues(discountViewModel);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.LogActivityAsync<Discount>(
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
            var discount = await DbContext.Discount.Include(d => d.Manager).FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {id} not found.");
            }

            var percentage = discount.Percentage;
            var creator = discount.Manager.UserName;
            using var transaction = DbContext.Database.BeginTransaction();
            try
            {
                // start tracking items to avoid FK constraint errors because Delete.ClientSetNull actually does NOT work
                await DbContext.Product.Where(d => d.DiscountId == id).LoadAsync();
                DbContext.Discount.Remove(discount);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Discount>(
                    managerId,
                    OperationType.Delete,
                    $"removed a {percentage} discount with ID {id} (created by {creator}).");

                await transaction.CommitAsync();
                return ServiceResult.Success();
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                return ServiceResult.Failure(ex.Message);
            }
        }

        public async Task<IEnumerable<Discount>> FindAllDiscountsAsync()
        {
            return (await DbContext.Discount.ToListAsync())
                .OrderBy(d =>
                {
                    var now = DateTime.Now;
                    return now > d.StartDate && now < d.EndDate;
                });
        }

        public async Task<Discount?> FindDiscountByIdAsync(int id)
        {
            return await DbContext.Discount.FindAsync(id);
        }

        public async Task<ServiceResult> UpdateDiscountAsync(string managerId, int id, DiscountViewModel updatedDiscountViewModel)
        {
            var discount = await DbContext.Discount.Include(d => d.Manager).FirstOrDefaultAsync(d => d.Id == id);
            if (discount == null)
            {
                return ServiceResult.Failure("Not found");
            }

            var entry = DbContext.Discount.Update(discount);
            entry.CurrentValues.SetValues(updatedDiscountViewModel);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Delete,
                $"removed discount with ID {id} (created by {discount.Manager.UserName}).");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> ApplyDiscountToProducts(string managerId, int discountId, IEnumerable<int> productIds)
        {
            var discount = await DbContext.Discount.FindAsync(discountId);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {discountId} was not found");
            }

            var appliedProducts = await DbContext.Product.Where(x => productIds.Contains(x.Id)).ToListAsync();
            foreach (var product in appliedProducts)
            {
                product.DiscountId = discountId;
            }

            DbContext.Product.UpdateRange(appliedProducts);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Modify,
                $"applied discount with ID {discountId} (created by {discount.Manager.UserName}) to the following products with IDs: {string.Join(", ", appliedProducts)}.");

            return ServiceResult.Success();
        }

        public async Task<ServiceResult> RemoveDiscountFromProductsAsync(string managerId, int discountId, IEnumerable<int> productIds)
        {
            var discount = await DbContext.Discount.FindAsync(discountId);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {discountId} was not found");
            }

            var appliedProducts = await DbContext.Product.Where(x => productIds.Contains(x.Id)).ToListAsync();
            var productsWithoutDiscount = appliedProducts.Where(p => p.DiscountId == null);
            if (productsWithoutDiscount.Any())
            {
                return ServiceResult.Failure($"Discount with ID {discountId} is not applied to product with the following IDs: {string.Join(", ", productsWithoutDiscount.Select(p => p.Id))}");
            }

            foreach (var product in appliedProducts)
            {
                product.DiscountId = null;
            }

            DbContext.Product.UpdateRange(appliedProducts);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.LogActivityAsync<Discount>(
                managerId,
                OperationType.Modify,
                $"removed discount with ID {discountId} (created by {discount.Manager.UserName}) from products with the following IDs: {string.Join(", ", appliedProducts)}.");

            return ServiceResult.Success();
        }
    }
}