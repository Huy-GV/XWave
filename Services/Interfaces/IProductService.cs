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
        // todo: add bool for including discontinued products
        Task<IEnumerable<DetailedProductDto>> GetAllProductsForStaff();
        Task<ProductDto> GetProductByIdForCustomers(int id);
        Task<DetailedProductDto> GetProductByIdForStaff(int id);
        Task<ServiceResult> CreateProductAsync(string staffId, ProductViewModel productViewModel);
        // todo: only update discontinued products
        Task<ServiceResult> UpdateProductAsync(string staffId, int id, ProductViewModel productViewModel);
        // todo: rename to discontinue
        Task<ServiceResult> DeleteProductAsync(string staffId, int id);
    }
}
