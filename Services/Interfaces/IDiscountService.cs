using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<IEnumerable<Discount>> GetAllAsync();
        Task<Discount> GetAsync(int id);
        Task<IEnumerable<Product>> GetProductsByDiscountId(int id);
        Task<ServiceResult> CreateAsync(string managerId, DiscountViewModel discount);
        Task<ServiceResult> UpdateAsync(string managerId, int id, DiscountViewModel discount);
        Task<ServiceResult> DeleteAsync(string managerId, int id);
    }
}
