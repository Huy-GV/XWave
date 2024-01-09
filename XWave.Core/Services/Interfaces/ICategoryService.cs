using XWave.Core.Models;
using XWave.Core.Services.Communication;

namespace XWave.Core.Services.Interfaces;

public interface ICategoryService
{
    Task<ServiceResult<int>> AddCategoryAsync(string managerUserId, Category category);

    Task<ServiceResult> UpdateCategoryAsync(string managerUserId, int id, Category category);

    Task<ServiceResult> DeleteCategoryAsync(string managerUserId, int id);

    Task<ServiceResult<Category>> FindCategoryByIdAsync(int id, string userId);

    Task<ServiceResult<IReadOnlyCollection<Category>>> FindAllCategoriesAsync(string userId);
}