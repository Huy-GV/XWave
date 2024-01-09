using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.Extension;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.Services.Interfaces;

namespace XWave.Core.Services.Implementations;

internal class CategoryService : ServiceBase, ICategoryService
{
    private readonly IStaffActivityLogger _activityService;
    private readonly IRoleAuthorizer _roleAuthorizer;

    private static readonly Error NonManagerUserError = Error.With(
        ErrorCode.AuthorizationError,
        "Only managers are authorized to modify Categories");

    private static readonly Error NonStaffUserError = Error.With(
        ErrorCode.AuthorizationError,
        "Only staff users are authorized to view Categories");

    public CategoryService(
        XWaveDbContext dbContext,
        IStaffActivityLogger activityService,
        IRoleAuthorizer roleAuthorizer) : base(dbContext)
    {
        _activityService = activityService;
        _roleAuthorizer = roleAuthorizer;
    }

    public async Task<ServiceResult<int>> AddCategoryAsync(string managerId, Category category)
    {
        if (!await _roleAuthorizer.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult<int>.Failure(NonManagerUserError);
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
            return ServiceResult<int>.UnknownFailure();
        }
    }

    public async Task<ServiceResult> DeleteCategoryAsync(string managerId, int id)
    {
        if (!await _roleAuthorizer.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult.Failure(NonManagerUserError);
        }

        var category = await DbContext.Category.FindAsync(id);
        if (category is null)
        {
            return ServiceResult.Failure(
                Error.With(
                    ErrorCode.EntityNotFound,
                    $"Category with ID {id} not found."));
        }

        try
        {
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
            return ServiceResult.UnknownFailure();
        }
    }

    public async Task<ServiceResult<IReadOnlyCollection<Category>>> FindAllCategoriesAsync(string userId)
    {
        if (!await _roleAuthorizer.IsUserInRoles(userId, new[] { RoleNames.Manager, RoleNames.Staff }))
        {
            return ServiceResult<IReadOnlyCollection<Category>>.Failure(NonStaffUserError);
        }

        var categories = await DbContext.Category.AsNoTracking().ToListAsync();

        return ServiceResult<IReadOnlyCollection<Category>>.Success(categories.AsIReadonlyCollection());
    }

    public async Task<ServiceResult<Category>> FindCategoryByIdAsync(int id, string userId)
    {
        if (!await _roleAuthorizer.IsUserInRoles(userId, new[] { RoleNames.Manager, RoleNames.Staff }))
        {
            return ServiceResult<Category>.Failure(NonStaffUserError);
        }

        var category = await DbContext.Category.FindAsync(id);
        if (category is null)
        {
            return ServiceResult<Category>.Failure(Error.With(ErrorCode.EntityNotFound));
        }

        return ServiceResult<Category>.Success(category);
    }

    public async Task<ServiceResult> UpdateCategoryAsync(
        string managerId,
        int id,
        Category updatedCategory)
    {
        if (!await _roleAuthorizer.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult.Failure(NonManagerUserError);
        }

        var category = await DbContext.Category.FindAsync(id);
        if (category is null)
        {
            return ServiceResult.Failure(
                Error.With(
                    ErrorCode.EntityNotFound,
                    $"Category with ID {id} not found."));
        }

        try
        {
            DbContext.Category.Update(category);
            category.Description = updatedCategory.Description;
            category.Name = updatedCategory.Name;
            await DbContext.SaveChangesAsync();
            await _activityService.LogActivityAsync<Category>(
                managerId,
                OperationType.Modify,
                $"updated category named {category.Name}");

            return ServiceResult.Success();
        }
        catch
        {
            return ServiceResult.UnknownFailure();
        }
    }
}
