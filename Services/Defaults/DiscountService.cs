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

        public DiscountService(
            XWaveDbContext dbContext,
            IActivityService staffActivityService) : base(dbContext)
        {
            _staffActivityService = staffActivityService;
        }

        public async Task<ServiceResult> CreateDiscountAsync(string managerId, DiscountViewModel discountViewModel)
        {
            var newDiscount = new Discount() { ManagerId = managerId };
            var entry = DbContext.Add(newDiscount);
            entry.CurrentValues.SetValues(discountViewModel);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.LogActivityAsync<Discount>(managerId, OperationType.Create);

            return ServiceResult.Success(newDiscount.Id.ToString());
        }

        public async Task<IEnumerable<Product>> FindProductsWithDiscountIdAsync(int discountId)
        {
            return await DbContext.Product
                .Where(p => p.DiscountId == discountId)
                .ToListAsync();
        }

        public async Task<ServiceResult> RemoveDiscountAsync(string managerID, int id)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {id} not found");
            }

            using var transaction = DbContext.Database.BeginTransaction();
            try
            {
                // start tracking items to avoid FK constraint errors because Delete.ClientSetNull actually does NOT work
                await DbContext.Product.Where(d => d.DiscountId == id).LoadAsync();
                DbContext.Discount.Remove(discount);
                await DbContext.SaveChangesAsync();
                var result = await _staffActivityService.LogActivityAsync<Discount>(managerID, OperationType.Delete);
                if (result.Succeeded)
                {
                    await transaction.CommitAsync();
                    return ServiceResult.Success(id.ToString());
                }

                return ServiceResult.Failure(result.Error);
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

        public async Task<Discount> FindDiscountByIdAsync(int id)
        {
            return await DbContext.Discount.FindAsync(id);
        }

        public async Task<ServiceResult> UpdateDiscountAsync(string managerID, int id, DiscountViewModel updatedDiscountViewModel)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            if (discount == null)
            {
                return ServiceResult.Failure("Not found");
            }

            var entry = DbContext.Discount.Update(discount);
            entry.CurrentValues.SetValues(updatedDiscountViewModel);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.LogActivityAsync<Discount>(managerID, OperationType.Modify);

            return ServiceResult.Success(id.ToString());
        }
    }
}