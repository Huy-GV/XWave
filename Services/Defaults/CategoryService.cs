using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using XWave.Data;
using XWave.Models;
using XWave.Services.Interfaces;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Defaults
{
    public class CategoryService : ServiceBase, ICategoryService
    {
        private readonly IActivityService _activityService;

        public CategoryService(
            XWaveDbContext dbContext,
            IActivityService activityService) : base(dbContext)
        {
            _activityService = activityService;
        }

        public async Task<(ServiceResult, int? CategoryId)> AddCategoryAsync(string managerUserName, Category category)
        {
            try
            {
                DbContext.Category.Add(category);
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Category>(
                    managerUserName,
                    OperationType.Create,
                    $"created a category named {category.Name}");

                return (ServiceResult.Success(), category.Id);
            }
            catch (Exception e)
            {
                return (ServiceResult.InternalFailure(), null);
            }
        }

        public async Task<ServiceResult> DeleteCategoryAsync(string managerId, int id)
        {
            try
            {
                var category = await DbContext.Category.FindAsync(id);
                if (category == null)
                {
                    return ServiceResult.Failure($"Category with ID {id} not found.");
                }
                
                var categoryName = category.Name;
                DbContext.Category.Remove(category);
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Category>(
                    managerId,
                    OperationType.Delete,
                    $"removed category named {categoryName}");
                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.InternalFailure();
            }
        }

        public async Task<IEnumerable<Category>> FindAllCategoriesAsync()
        {
            return await DbContext.Category.AsNoTracking().ToListAsync();
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
                if (category == null)
                {
                    return ServiceResult.Failure($"Category with ID {id} not found.");
                }

                category.Description = updatedCategory.Description;
                category.Name = updatedCategory.Name;
                DbContext.Category.Update(category);
                await DbContext.SaveChangesAsync();
                await _activityService.LogActivityAsync<Category>(
                    managerId,
                    OperationType.Modify,
                    $"updated category named {category.Name}");

                return ServiceResult.Success();
            }
            catch (Exception e)
            {
                return ServiceResult.InternalFailure();
            }
        }
    }
}