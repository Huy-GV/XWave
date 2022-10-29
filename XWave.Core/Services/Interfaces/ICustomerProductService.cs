using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface ICustomerProductService
{
    /// <summary>
    ///     Find all products with limited details for customers.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>A DTO containing limited details of the product.</returns>
    Task<IReadOnlyCollection<ProductDto>> FindAllProducts();

    /// <summary>
    ///     Find a product by its ID with limited details for customers.
    /// </summary>
    /// <param name="id">Product ID.</param>
    /// <returns>A DTO containing limited details of the product.</returns>
    Task<ProductDto?> FindProduct(int id);
}