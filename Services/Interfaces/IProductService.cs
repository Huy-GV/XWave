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
        Task<IEnumerable<ProductDTO>> GetAllProductsForCustomers();
        Task<IEnumerable<StaffProductDTO>> GetAllProductsForStaff();
        Task<ProductDTO> GetProductByIDForCustomers(int id);
        Task<StaffProductDTO> GetProductByIDForStaff(int id);
        Task<ServiceResult> CreateProductAsync(string staffID, ProductViewModel productViewModel);
        Task<ServiceResult> UpdateProductAsync(string staffID, int id, ProductViewModel productViewModel);
        Task<ServiceResult> DeleteProductAsync(string staffID, int id);
    }
}
