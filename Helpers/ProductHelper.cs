using System;
using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Models;

namespace XWave.Helpers
{
    public class ProductHelper
    {
        public StaffProductDTO? CreateStaffProductDTO(Product? product)
        {
            if (product == null)
            {
                return null;
            }

            var staffDiscountDTO = CreateStaffDiscountDTO(product);
            return new StaffProductDTO
            {
                ID = product.Id,
                ProductName = product.Name,
                CategoryName = product.Category.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryID = product.Category.Id,
                LatestRestock = product.LastRestock,
                Discount = staffDiscountDTO
            };
        }
        public ProductDTO? CreateCustomerProductDTO(Product? product)
        {
            if (product == null)
            {
                return null;
            }
            DiscountDTO? discountDTO = CreateCustomerDiscountDTO(product);

            return new ProductDTO()
            {
                ID = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = product.Quantity,
                CategoryID = product.CategoryID,
                CategoryName = product.Category.Name,
                Discount = discountDTO
            };
        }
        private DiscountDTO? CreateCustomerDiscountDTO(Product product)
        {
            if (product.Discount != null)
            {
                var currentDate = DateTime.Now;
                var isActive = product.Discount.StartDate < currentDate && product.Discount.EndDate > currentDate;

                return new DiscountDTO
                {
                    Percentage = product.Discount.Percentage,
                    IsActive = isActive,
                    EndDate = product.Discount.EndDate,
                };
            }

            return null;
        }
        private StaffDiscountDTO? CreateStaffDiscountDTO(Product product)
        {
            if (product.Discount != null)
            {
                return new StaffDiscountDTO()
                {
                    ManagerID = product.Discount.ManagerId,
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
