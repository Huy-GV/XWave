using System;
using System.Collections.Generic;
using XWave.Models;

namespace XWave.DTOs.Customers
{
    public class DiscountDTO
    {
        public uint Percentage { get; set; }
        public bool IsActive { get; set; }
        public DateTime EndDate { get; set; }
    }
}
