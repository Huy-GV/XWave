using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> FindAllProductsForCustomers();

        Task<IEnumerable<DetailedProductDto>> FindAllProductsForStaff(bool includeDiscontinuedProducts = false);

        /// <summary>
        /// Find a product by its ID with limited details for customers.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <returns>A DTO containing limited details of the product.</returns>
        Task<ProductDto?> FindProductByIdForCustomers(int id);

        /// <summary>
        /// Find a product by its ID with full details for staff.
        /// </summary>
        /// <param name="id">Product ID.</param>
        /// <returns>A DTO containing details of the product.</returns>
        Task<DetailedProductDto?> FindProductByIdForStaff(int id);

        Task<(ServiceResult, int? ProductId)> AddProductAsync(string staffId, ProductViewModel productViewModel);

        /// <summary>
        /// Update general information (Name, Description, and Price) of a product.
        /// </summary>
        /// <param name="staffId">ID of staff user who issued the update.</param>
        /// <param name="id">ID of updated product.</param>
        /// <param name="productViewModel">ViewModel containing details of the updated product.</param>
        /// <returns></returns>
        Task<ServiceResult> UpdateProductAsync(string staffId, int id, ProductViewModel productViewModel);

        /// <summary>
        /// Update existing stock due to restocking or erroreneous input.
        /// </summary>
        /// <param name="productId">ID of product to update.</param>
        /// <param name="updatedStockQuantity">New stock quantity.</param>
        /// <returns></returns>
        Task<ServiceResult> UpdateStockAsync(string staffId, int productId, uint updatedStockQuantity);

        Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice, DateTime updateSchedule);

        Task<ServiceResult> UpdateProductPriceAsync(string staffId, int productId, uint updatedPrice);

        /// <summary>
        /// Soft-delete a product.
        /// </summary>
        /// <param name="id">ID of the product to delete.</param>
        /// <returns></returns>
        Task<ServiceResult> DeleteProductAsync(int id);

        // todo: make the below methods accept an array of product
        /// <summary>
        /// Restart sale of a discontinued product.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateSchedule">The scheduled date for the restart.</param>
        /// <returns></returns>
        Task<ServiceResult> RestartProductSaleAsync(int id, DateTime updateSchedule);

        /// <summary>
        /// Discontinue a product.
        /// </summary>
        /// <param name="id"></param>
        /// <param name="updateSchedule">The scheduled date for discontinuing an active product.</param>
        /// <returns></returns>
        Task<ServiceResult> DiscontinueProductAsync(int id, DateTime updateSchedule);
    }
}