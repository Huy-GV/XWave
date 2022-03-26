using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDto>> GetAllProductsForCustomers();
        Task<IEnumerable<DetailedProductDto>> GetAllProductsForStaff(bool includeDiscontinuedProducts = false);
        Task<ProductDto> GetProductByIdForCustomers(int id);
        Task<DetailedProductDto> GetProductByIdForStaff(int id);
        // todo: replace create with add in services
        Task<ServiceResult> AddProductAsync(string staffId, ProductViewModel productViewModel);
        // todo: only update discontinued products and only update general information
        Task<ServiceResult> UpdateProductAsync(string staffId, int id, ProductViewModel productViewModel);
        /// <summary>
        /// Update existing stock due to restocking or erroreneous input.
        /// </summary>
        /// <param name="productId">ID of product to update.</param>
        /// <param name="updatedStock">New stock quantity.</param>
        /// <returns></returns>
        Task<ServiceResult> UpdateStockAsync(int productId, uint updatedStock);
        Task<ServiceResult> DeleteProductAsync(int id);
        Task<ServiceResult> UpdateDiscontinuationStatus(int id, bool isDiscontinued);
    }
}
