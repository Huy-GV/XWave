﻿using Microsoft.AspNetCore.Identity;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Data;
using XWave.Data.Constants;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;
using Microsoft.EntityFrameworkCore;
using System;

namespace XWave.Services.Defaults
{
    public class DiscountService : ServiceBase, IDiscountService
    {
        private readonly IStaffActivityService _staffActivityService;
        public DiscountService(
            XWaveDbContext dbContext,
            IStaffActivityService staffActivityService) : base(dbContext)  
        {
            _staffActivityService = staffActivityService;
        }
        public async Task<ServiceResult> CreateAsync(string managerID, DiscountViewModel discount)
        {
            var newDiscount = new Discount()
            {
                EndDate = discount.EndDate,
                StartDate = discount.StartDate,
                Percentage = discount.Percentage,
                ManagerID = managerID
            };

            DbContext.Add(newDiscount);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.CreateLog<Discount>(managerID, OperationType.Create);
            return ServiceResult.Success(newDiscount.ID.ToString());
        }
        public async Task<IEnumerable<Product>> GetProductsByDiscountID(int discountID)
        {
            return await DbContext.Product
                .Where(p => p.DiscountID == discountID)
                .ToListAsync();
        }

        public async Task<ServiceResult> DeleteAsync(string managerID, int id)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            if (discount == null)
            {
                return ServiceResult.Failure($"Discount with ID {id} not found");
            }

            using var transaction = DbContext.Database.BeginTransaction();
            string savepoint = "BeforeDiscountRemoval";
            transaction.CreateSavepoint(savepoint);
            try
            {
                //start tracking items to avoid FK constraint errors
                await DbContext.Product.Where(d => d.DiscountID == id).ToListAsync();

                DbContext.Discount.Remove(discount);
                await DbContext.SaveChangesAsync();

                transaction.Commit();
                await _staffActivityService.CreateLog<Discount>(managerID, OperationType.Delete);
                return ServiceResult.Success(id.ToString());
            }
            catch (Exception ex)
            {
                transaction.RollbackToSavepoint(savepoint);
                return ServiceResult.Failure(ex.Message);
            }
        }

        public async Task<IEnumerable<Discount>> GetAllAsync()
        {
            return await DbContext.Discount.ToListAsync();
        }

        public async Task<Discount> GetAsync(int id)
        {
            return await DbContext.Discount.FindAsync(id);
        }

        public async Task<ServiceResult> UpdateAsync(string managerID, int id, DiscountViewModel updatedDiscount)
        {
            var discount = await DbContext.Discount.FindAsync(id);
            if (discount == null)
            {
                return ServiceResult.Failure("Not found");
            }

            var entry = DbContext.Attach(discount);
            entry.State = EntityState.Modified;
            entry.CurrentValues.SetValues(updatedDiscount);
            await DbContext.SaveChangesAsync();
            await _staffActivityService.CreateLog<Discount>(managerID, OperationType.Modify);

            return ServiceResult.Success(id.ToString());
        }
    }
}
