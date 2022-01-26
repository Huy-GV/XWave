using System.Collections.Generic;
using System.Threading.Tasks;
using XWave.DTOs;
using XWave.Models;
using XWave.Services.ResultTemplate;
using XWave.ViewModels.Management;

namespace XWave.Services.Interfaces
{
    public interface IProductService
    {
        Task<IEnumerable<ProductDTO>> GetAllProductsForCustomers(int? categoryID = null);
        Task<IEnumerable<Product>> GetAllProductsForStaff(int? categoryID = null);
        Task<ProductDTO> GetProductByIDForCustomers(int id);
        Task<Product> GetProductByIDForStaff(int id);
        Task<ServiceResult> CreateProductAsync(ProductVM productVM);
        Task<ServiceResult> UpdateProductAsync(int id, ProductVM productVM);
        Task<ServiceResult> DeleteProductAsync(int id);
    }
}
