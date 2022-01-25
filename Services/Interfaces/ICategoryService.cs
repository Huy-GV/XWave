using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult> CreateCategory(Category category);
        Task<ServiceResult> UpdateCategory(int id, Category category);
        Task<ServiceResult> DeleteCategory(int id);
        Task<Category> GetCategoryByID(int id);
        Task<IEnumerable<Category>> GetAllCategories();

    }
}
