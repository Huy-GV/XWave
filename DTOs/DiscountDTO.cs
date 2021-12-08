using System;
using System.Collections.Generic;
using XWave.Models;

namespace XWave.DTOs
{
    public class DiscountDTO
    {
        public uint Percentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime EndDate { get; set; }
        public static DiscountDTO From(Discount discount)
        {
            return new DiscountDTO
            {
                Percentage = discount.Percentage,
                IsActive = discount.IsActive,
                EndDate = discount.EndDate,
            };
        }
    }
}
