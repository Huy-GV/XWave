using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;

namespace XWave.Core.Utils;

public class ProductDtoMapper
{
    public DetailedProductDto MapDetailedProductDto(Product product)
    {
        return new DetailedProductDto
        {
            Id = product.Id,
            ProductName = product.Name,
            CategoryName = product.Category.Name,
            Price = product.Price,
            IsDiscontinued = product.IsDiscontinued,
            Quantity = product.Quantity,
            CategoryId = product.Category.Id,
            LatestRestock = product.LatestRestock,
            Discount = product.Discount is null ? null : DiscountDtoMapper.MapDetailedDiscountDto(product.Discount)
        };
    }

    public ProductDto MapCustomerProductDto(Product product, decimal? discountedPrice = null)
    {
        var activeDiscountExists = 
            product.Discount is not null && 
            DateTime.Now.IsBetween(product.Discount.StartDate, product.Discount.EndDate);
        var discountDto = activeDiscountExists 
            ? DiscountDtoMapper.MapCustomerDiscountDto(product.Discount!)
            : null;

        return new ProductDto
        {
            Id = product.Id,
            Name = product.Name,
            Price = product.Price,
            Quantity = product.Quantity,
            CategoryId = product.CategoryId,
            CategoryName = product.Category.Name,
            Discount = discountDto,
            DiscountedPrice = discountedPrice
        };
    }
}