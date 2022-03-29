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

        Task<Discount?> FindDiscountByIdAsync(int id);

        Task<IEnumerable<Product>> FindProductsWithDiscountIdAsync(int id);

        Task<(ServiceResult, int? DiscountId)> CreateDiscountAsync(string managerId, DiscountViewModel discount);

        Task<ServiceResult> UpdateDiscountAsync(string managerId, int id, DiscountViewModel discount);

        Task<ServiceResult> RemoveDiscountAsync(string managerId, int id);

        /// <summary>
        /// Apply a discount to a collection of products.
        /// </summary>
        /// <param name="discountId">ID of the discount.</param>
        /// <param name="productIds">Collection of IDs of products to apply the discount to.</param>
        /// <returns></returns>
        Task<ServiceResult> ApplyDiscountToProducts(int discountId, IEnumerable<int> productIds);

        /// <summary>
        /// Remove a discount from a collection of products on which the discount is applied.
        /// </summary>
        /// <param name="discountId">ID of the discount.</param>
        /// <param name="productIds">Collection of IDs of products to remove the discount from.</param>
        /// <returns></returns>
        Task<ServiceResult> RemoveDiscountFromProductsAsync(int discountId, IEnumerable<int> productIds);
    }
}