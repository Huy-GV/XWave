using XWave.DTOs.Customers;
using XWave.DTOs.Management;
using XWave.Models;

namespace XWave.Utils;

public class DiscountDtoMapper
{
    public static DiscountDto MapCustomerDiscountDto(Discount discount)
    {
        return new()
        {
            Percentage = discount.Percentage,
            EndDate = discount.EndDate
        };
    }

    public static DetailedDiscountDto MapDetailedDiscountDto(Discount discount)
    {
        return new()
        {
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            Percentage = discount.Percentage
        };
    }
}