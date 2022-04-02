using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<(ServiceResult, int? CategoryId)> AddCategoryAsync(string managerUserName, Category category);

        Task<ServiceResult> UpdateCategoryAsync(string managerUserName, int id, Category category);

        Task<ServiceResult> DeleteCategoryAsync(string managerUserName, int id);

        Task<Category?> FindCategoryByIdAsync(int id);

        Task<IEnumerable<Category>> FindAllCategoriesAsync();
    }
}