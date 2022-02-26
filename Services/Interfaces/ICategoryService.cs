using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult> CreateCategoryAsync(string managerId, Category category);
        Task<ServiceResult> UpdateCategoryAsync(string managerId, int id, Category category);
        Task<ServiceResult> DeleteCategoryAsync(string managerId, int id);
        Task<Category> GetCategoryById(int id);
        Task<IEnumerable<Category>> GetAllCategories();

    }
}
