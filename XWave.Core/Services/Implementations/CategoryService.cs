﻿using Microsoft.EntityFrameworkCore;
using XWave.Core.Data;
using XWave.Core.Data.Constants;
using XWave.Core.Extension;
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
        Code = ErrorCode.AuthorizationError,
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
        if (!await _authorizationService.IsUserInRole(managerId, RoleNames.Manager))
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
            return ServiceResult<int>.UnknownFailure();
        }
    }

    public async Task<ServiceResult> DeleteCategoryAsync(string managerId, int id)
    {
        if (!await _authorizationService.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var category = await DbContext.Category.FindAsync(id);
        if (category is null)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Category with ID {id} not found.",
            });
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

    public async Task<IReadOnlyCollection<Category>> FindAllCategoriesAsync()
    {
        var categories = await DbContext.Category.AsNoTracking().ToListAsync();

        return categories.AsIReadonlyCollection();
    }

    public async Task<Category?> FindCategoryByIdAsync(int id)
    {
        return await DbContext.Category.FindAsync(id);
    }

    public async Task<ServiceResult> UpdateCategoryAsync(
        string managerId, 
        int id, 
        Category updatedCategory)
    {
        if (!await _authorizationService.IsUserInRole(managerId, RoleNames.Manager))
        {
            return ServiceResult.Failure(_unauthorizedOperationError);
        }

        var category = await DbContext.Category.FindAsync(id);
        if (category is null)
        {
            return ServiceResult.Failure(new Error
            {
                Code = ErrorCode.EntityNotFound,
                Message = $"Category with ID {id} not found.",
            });
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