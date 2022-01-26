using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult> CreateCategoryAsync(Category category);
        Task<ServiceResult> UpdateCategoryAsync(int id, Category category);
        Task<ServiceResult> DeleteCategoryAsync(int id);
        Task<Category> GetCategoryByID(int id);
        Task<IEnumerable<Category>> GetAllCategories();

    }
}
