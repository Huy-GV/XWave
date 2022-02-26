using System;
using XWave.Models;

namespace XWave.DTOs.Management
{
    public class StaffProductDto
    {
        public int Id { get; init; }
        public string ProductName { get; init; }
        public string CategoryName { get; init; }
        public decimal Price { get; init; }
        public uint Quantity { get; init; }
        public DateTime LatestRestock { get; set; }
        public int CategoryId { get; init; }
        public StaffDiscountDto? Discount { get; init; }
    }
}
