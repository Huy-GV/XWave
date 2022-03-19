using Microsoft.EntityFrameworkCore;
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
        private readonly IActivityService _staffActivityService;
        public CategoryService(
            XWaveDbContext dbContext,
            IActivityService staffActivityService) : base(dbContext)
        { 
            _staffActivityService = staffActivityService;
        }
        public async Task<ServiceResult> CreateCategoryAsync(string managerID, Category category)
        {
            try
            {
                DbContext.Category.Add(category);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Category>(managerID, OperationType.Create);

                return ServiceResult.Success(category.Id.ToString());
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }
        }

        public async Task<ServiceResult> DeleteCategoryAsync(string managerID, int id)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                DbContext.Category.Remove(category);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Category>(managerID, OperationType.Delete);
                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }
        }
        public Task<IEnumerable<Category>> FindAllCategoriesAsync()
        {
            return Task.FromResult(DbContext.Category.AsEnumerable());
        }

        public async Task<Category> FindCategoryByIdAsync(int id)
        {
            return await DbContext.Category.FindAsync(id);
        }

        public async Task<ServiceResult> UpdateCategoryAsync(string managerID, int id, Category updatedCategory)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                category.Description = updatedCategory.Description;
                category.Name = updatedCategory.Name;
                DbContext.Category.Update(category);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Category>(managerID, OperationType.Modify);
                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }

        }
    }
}
