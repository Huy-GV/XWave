﻿using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Defaults
{
    public class CategoryService : ServiceBase, ICategoryService
    {
        public CategoryService(XWaveDbContext dbContext) : base(dbContext) { }
        public async Task<ServiceResult> CreateCategory(Category category)
        {
            DbContext.Category.Add(category);
            await DbContext.SaveChangesAsync();
            return ServiceResult.Success(category.ID.ToString());
        }

        public async Task<ServiceResult> DeleteCategory(int id)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                DbContext.Category.Remove(category);
                await DbContext.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }
        }
        public Task<IEnumerable<Category>> GetAllCategories()
        {
            return Task.FromResult(DbContext.Category.AsEnumerable());
        }

        public async Task<Category> GetCategoryByID(int id)
        {
            return await DbContext.Category.FindAsync(id);
        }

        public async Task<ServiceResult> UpdateCategory(int id, Category updatedCategory)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                category.Description = updatedCategory.Description;
                category.Name = updatedCategory.Name;
                DbContext.Category.Update(category);
                await DbContext.SaveChangesAsync();
                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }

        }
    }
}