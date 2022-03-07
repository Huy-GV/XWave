using System;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Models;

namespace XWave.Helpers
{
    public class ProductHelper
    {
        public DetailedProductDto CreateDetailedProductDto(Product product)
        {
            var staffDiscountDTO = CreateDetailedDiscountDto(product);

            return new DetailedProductDto
            {
                Id = product.Id,
                ProductName = product.Name,
                CategoryName = product.Category.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.Category.Id,
                LatestRestock = product.LastRestock,
                Discount = staffDiscountDTO
            };
        }
        public ProductDto CreateCustomerProductDTO(Product product)
        {
            var discountDTO = CreateCustomerDiscountDto(product);

            return new ProductDto()
            {
                Id = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryId = product.CategoryId,
                CategoryName = product.Category.Name,
                Discount = discountDTO
            };
        }
        private DiscountDto? CreateCustomerDiscountDto(Product product)
        {
            if (product.Discount != null)
            {
                var currentDate = DateTime.Now;
                var isActive = product.Discount.StartDate < currentDate && product.Discount.EndDate > currentDate;

                return new DiscountDto
                {
                    Percentage = product.Discount.Percentage,
                    IsActive = isActive,
                    EndDate = product.Discount.EndDate,
                };
            }

            return null;
        }
        private DetailedDiscountDto? CreateDetailedDiscountDto(Product product)
        {
            if (product.Discount != null)
            {
                return new DetailedDiscountDto()
                {
                    ManagerName = product.Discount.Manager.UserName,
                    StartDate = product.Discount.StartDate,
                    EndDate = product.Discount.EndDate,
                    Percentage = product.Discount.Percentage,
                };
            }

            return null;
        }
    }
}
