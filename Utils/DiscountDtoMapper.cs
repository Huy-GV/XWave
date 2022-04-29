using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Models;

namespace XWave.Utils
{
    public class DiscountDtoMapper
    {
        public static DiscountDto MapCustomerDiscountDto(Discount discount) => new()
        {
            Percentage = discount.Percentage,
            EndDate = discount.EndDate,
        };

        public static DetailedDiscountDto MapDetailedDiscountDto(Discount discount) => new()
        {
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            Percentage = discount.Percentage,
        };
    }
}