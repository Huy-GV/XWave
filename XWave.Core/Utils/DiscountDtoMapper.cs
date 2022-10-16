using XWave.Core.DTOs.Customers;
using XWave.Core.DTOs.Management;
using XWave.Core.Models;

namespace XWave.Core.Utils;

public static class DiscountDtoMapper
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
            Id = discount.Id,
            StartDate = discount.StartDate,
            EndDate = discount.EndDate,
            Percentage = discount.Percentage
        };
    }
}