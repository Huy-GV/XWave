using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Models;
using XWave.Core.Services.Interfaces;
using XWave.Core.Services.ResultTemplate;

namespace XWave.Core.Services.Implementations;

internal class CategoryService : ServiceBase, ICategoryService
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
        catch
        {
            return (ServiceResult.InternalFailure(), null);
        }
    }

    public async Task<ServiceResult> DeleteCategoryAsync(string managerId, int id)
    {
        try
        {
            var category = await DbContext.Category.FindAsync(id);
            if (category == null) return ServiceResult.Failure($"Category with ID {id} not found.");

            var categoryName = category.Name;
            DbContext.Category.Remove(category);
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Category>(
                managerId,
                OperationType.Delete,
                $"removed category named {categoryName}");
            return ServiceResult.Success();
        }
        catch
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
            if (category == null) return ServiceResult.Failure($"Category with ID {id} not found.");

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
        catch
        {
            return ServiceResult.InternalFailure();
        }
    }
}