using XWave.Core.DTOs.Management;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface IDiscountService
{
    Task<IEnumerable<DetailedDiscountDto>> FindAllDiscountsAsync();

    Task<DetailedDiscountDto?> FindDiscountByIdAsync(int id);

    Task<IEnumerable<Product>> FindProductsWithDiscountIdAsync(int id);

    Task<ServiceResult<int>> CreateDiscountAsync(string managerId, DiscountViewModel discount);

    Task<ServiceResult> UpdateDiscountAsync(string managerId, int id, DiscountViewModel discount);

    Task<ServiceResult> RemoveDiscountAsync(string managerId, int id);

    /// <summary>
    ///     Apply a discount to a collection of products.
    /// </summary>
    /// <param name="discountId">ID of the discount.</param>
    /// <param name="productIds">Collection of IDs of products to apply the discount to.</param>
    /// <returns></returns>
    Task<ServiceResult> ApplyDiscountToProducts(string managerId, int discountId, IEnumerable<int> productIds);

    /// <summary>
    ///     Remove a discount from a collection of products on which the discount is applied.
    /// </summary>
    /// <param name="discountId">ID of the discount.</param>
    /// <param name="productIds">Collection of IDs of products to remove the discount from.</param>
    /// <returns></returns>
    Task<ServiceResult> RemoveDiscountFromProductsAsync(string managerId, int discountId, IEnumerable<int> productIds);

    // todo: schedule discount removal
}