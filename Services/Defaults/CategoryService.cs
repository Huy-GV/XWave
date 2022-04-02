using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
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

        public async Task<(ServiceResult, int? CategoryId)> AddCategoryAsync(string managerUserName, Category category)
        {
            try
            {
                DbContext.Category.Add(category);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Category>(
                    managerUserName,
                    OperationType.Create,
                    $"created a category named {category.Name}");

                return (ServiceResult.Success(), category.Id);
            }
            catch (Exception e)
            {
                return (ServiceResult.Failure(e.Message), null);
            }
        }

        public async Task<ServiceResult> DeleteCategoryAsync(string managerId, int id)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                var categoryName = category.Name;
                DbContext.Category.Remove(category);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Category>(
                    managerId,
                    OperationType.Delete,
                    $"removed category named {categoryName}");
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

        public async Task<Category?> FindCategoryByIdAsync(int id)
        {
            return await DbContext.Category.FindAsync(id);
        }

        public async Task<ServiceResult> UpdateCategoryAsync(string managerId, int id, Category updatedCategory)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                category.Description = updatedCategory.Description;
                category.Name = updatedCategory.Name;
                DbContext.Category.Update(category);
                await DbContext.SaveChangesAsync();
                await _staffActivityService.LogActivityAsync<Category>(
                    managerId,
                    OperationType.Modify,
                    $"updated category named {category.Name}");

                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.Failure(e.Message);
            }
        }
    }
}