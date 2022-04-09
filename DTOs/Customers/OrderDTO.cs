using System;
using System.Collections.Generic;

namespace XWave.DTOs.Customers
{
    public record OrderDto
    {
        public IEnumerable<OrderDetailDto> OrderDetails { get; set; }
        public DateTime OrderDate { get; set; }
        public string AccountNo { get; set; }
        public int Id { get; set; }
    }
}