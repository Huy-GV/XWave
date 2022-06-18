using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Extension;
using XWave.Core.Models;

namespace XWave.Core.Utils;

public class ProductDtoMapper
{
    public static DetailedProductDto MapDetailedProductDto(Product product)
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
            Discount = product.Discount == null ? null : DiscountDtoMapper.MapDetailedDiscountDto(product.Discount)
        };
    }

    public static ProductDto MapCustomerProductDto(Product product, decimal? discountedPrice = null)
    {
        DiscountDto? discountDto = null;
        if (product.Discount != null && DateTime.Now.IsBetween(product.Discount.StartDate, product.Discount.EndDate))
            discountDto = DiscountDtoMapper.MapCustomerDiscountDto(product.Discount);

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