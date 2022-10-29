using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Models;
using XWave.Core.Services.Communication;
using XWave.Core.ViewModels.Management;

namespace XWave.Core.Services.Interfaces;

public interface ICustomerProductService
{
    /// <summary>
    ///     Calculate the discounted price of a product. Throws InvalidOperationException if product does not have any discount
    /// </summary>
    /// <param name="product">Product with a discount</param>
    /// <returns>Discounted price</returns>
    decimal CalculatePriceAfterDiscount(Product product);

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