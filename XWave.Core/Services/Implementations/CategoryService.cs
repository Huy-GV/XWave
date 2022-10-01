using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class CategoryService : ServiceBase, ICategoryService
{
    private readonly IActivityService _activityService;
    private readonly IAuthorizationService _authorizationService;

    private readonly Error _unauthorizedOperationError = new()
    {
        ErrorCode = ErrorCode.InvalidUserRequest,
        Message = "Only managers are authorized to modify Categories",
    };

    public CategoryService(
        XWaveDbContext dbContext,
        IActivityService activityService,
        IAuthorizationService authorizationService) : base(dbContext)
    {
        _activityService = activityService;
        _authorizationService = authorizationService;
    }

    public async Task<ServiceResult<int>> AddCategoryAsync(string managerId, Category category)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult<int>.Failure(_unauthorizedOperationError);
        }

        try
        {
            DbContext.Category.Add(category);
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Category>(
                managerId,
                OperationType.Create,
                $"created a category named {category.Name}");

            return ServiceResult<int>.Success(category.Id);
        }
        catch
        {
            return ServiceResult<int>.DefaultFailure();
        }
    }

    public async Task<ServiceResult> DeleteCategoryAsync(string managerId, int id)
    {
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        try
        {
            var category = await DbContext.Category.FindAsync(id);
            if (category is null)
            {
                return ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = $"Category with ID {id} not found.",
                });
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
        catch
        {
            return ServiceResult.DefaultFailure();
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
        if (!await _authorizationService.IsUserInRole(managerId, Data.Constants.Roles.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        try
        {
            var category = await DbContext.Category.FindAsync(id);
            if (category is null)
            {
                return ServiceResult.Failure(new Error
                {
                    ErrorCode = ErrorCode.EntityNotFound,
                    Message = $"Category with ID {id} not found.",
                });
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
        catch
        {
            return ServiceResult.DefaultFailure();
        }
    }
}