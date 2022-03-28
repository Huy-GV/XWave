using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IDiscountService
    {
        Task<IEnumerable<Discount>> FindAllDiscountsAsync();

        Task<Discount> FindDiscountByIdAsync(int id);

        Task<IEnumerable<Product>> FindProductsWithDiscountIdAsync(int id);

        Task<ServiceResult> CreateDiscountAsync(string managerId, DiscountViewModel discount);

        Task<ServiceResult> UpdateDiscountAsync(string managerId, int id, DiscountViewModel discount);

        Task<ServiceResult> RemoveDiscountAsync(string managerId, int id);

        Task<ServiceResult> ApplyDiscountToProducts(int discountId, IEnumerable<int> productIds);

        Task<ServiceResult> RemoveDiscountFromProductsAsync(int discountId, IEnumerable<int> productIds);
    }
}