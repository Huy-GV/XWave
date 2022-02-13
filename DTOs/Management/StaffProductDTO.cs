using System;
using XWave.Models;

namespace XWave.DTOs.Management
{
    public class StaffProductDTO
    {
        public int ID { get; init; }
        public string ProductName { get; init; }
        public string CategoryName { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
        public DateTime LatestRestock { get; set; }
        public int CategoryID { get; init; }
        public StaffDiscountDTO? Discount { get; init; }
    }
}
