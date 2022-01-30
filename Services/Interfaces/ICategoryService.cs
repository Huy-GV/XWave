using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;

namespace XWave.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<ServiceResult> CreateCategoryAsync(string managerID, Category category);
        Task<ServiceResult> UpdateCategoryAsync(string managerID, int id, Category category);
        Task<ServiceResult> DeleteCategoryAsync(string managerID, int id);
        Task<Category> GetCategoryByID(int id);
        Task<IEnumerable<Category>> GetAllCategories();

    }
}
