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
        Task<ServiceResult> CreateAsync(string managerID, DiscountVM discount);
        Task<ServiceResult> UpdateAsync(int id, DiscountVM discount);
        Task<ServiceResult> DeleteAsync(int id);
    }
}
