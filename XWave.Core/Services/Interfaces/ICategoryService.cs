using XWave.Core.Models;
using XWave.Core.Services.Communication;

namespace XWave.Core.Services.Interfaces;

public interface ICategoryService
{
    Task<(ServiceResult, int? CategoryId)> AddCategoryAsync(string managerUserName, Category category);

    Task<ServiceResult> UpdateCategoryAsync(string managerUserName, int id, Category category);

    Task<ServiceResult> DeleteCategoryAsync(string managerUserName, int id);

    Task<Category?> FindCategoryByIdAsync(int id);

    Task<IEnumerable<Category>> FindAllCategoriesAsync();
}