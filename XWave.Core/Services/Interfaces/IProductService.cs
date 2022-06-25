using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface IProductService
{
    /// <summary>
    ///     Calculate the discounted price of a product. Throws InvalidOperationException if product does not have any discount
    /// </summary>
    /// <param name="product">Product with a discount</param>
    /// <returns>Discounted price</returns>
    decimal CalculateDiscountedPrice(Product product);

    Task<IEnumerable<ProductDto>> FindAllProductsForCustomers();

    Task<IEnumerable<DetailedProductDto>> FindAllProductsForStaff(bool includeDiscontinuedProducts = false);

    /// <summary>
    ///     Find a product by its ID with limited details for customers.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>A DTO containing limited details of the product.</returns>
    Task<ProductDto?> FindProductByIdForCustomers(int id);

    /// <summary>
    ///     Find a product by its ID with full details for staff.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>A DTO containing details of the product.</returns>
    Task<DetailedProductDto?> FindProductByIdForStaff(int id);

    Task<(ServiceResult, int? ProductId)> AddProductAsync(string staffId, ProductViewModel productViewModel);

    /// <summary>
    ///     Update general information (Name, Description, and Price) of a product.
    /// </summary>
    /// <param name="staffId">ID of staff user who issued the update.</param>
    /// <param name="id">ID of updated product.</param>
    /// <param name="productViewModel">ViewModel containing details of the updated product.</param>
    /// <returns></returns>
    Task<ServiceResult> UpdateProductAsync(string staffId, int id, ProductViewModel productViewModel);

    /// <summary>
    ///     Update existing stock due to restocking or erroreneous input.
    /// </summary>
    /// <param name="productId">ID of product to update.</param>
    /// <param name="updatedStockQuantity">New stock quantity.</param>
    /// <returns></returns>
    Task<ServiceResult> UpdateStockAsync(string staffId, int productId, uint updatedStockQuantity);

    Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice,
        DateTime updateSchedule);

    Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice);

    /// <summary>
    ///     Soft-delete a product.
    /// </summary>
    /// <param name="id">ID of the product to delete.</param>
    /// <param name="managerId">ID of the manager who deleted discount.</param>
    /// <returns></returns>
    Task<ServiceResult> DeleteProductAsync(int id, string managerId);

    /// <summary>
    ///     Restart sale of a discontinued product.
    /// </summary>
    /// ///
    /// <param name="userId"></param>
    /// <param name="id"></param>
    /// <param name="updateSchedule">The scheduled date for the restart.</param>
    /// <returns></returns>
    Task<ServiceResult> RestartProductSaleAsync(string userId, int id, DateTime updateSchedule);

    /// <summary>
    ///     Discontinue a product.
    /// </summary>
    /// <param name="userId"></param>
    /// <param name="id"></param>
    /// <param name="updateSchedule">The scheduled date for discontinuing an active product.</param>
    /// <returns></returns>
    Task<ServiceResult> DiscontinueProductAsync(string userId, int[] ids, DateTime updateSchedule);
}