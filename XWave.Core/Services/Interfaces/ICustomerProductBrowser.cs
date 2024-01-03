using XWave.Core.DTOs.Customers;

namespace XWave.Core.Services.Interfaces;

public interface ICustomerProductBrowser
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
