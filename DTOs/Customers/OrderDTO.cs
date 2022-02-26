using System.Collections.Generic;
using System;
using XWave.Models;

namespace XWave.DTOs.Customers
{
    public class OrderDto
    {
        public  IEnumerable<OrderDetailDto> OrderDetailCollection { get; set; }   
        public DateTime OrderDate { get; set; }
        public string AccountNo { get; set; }
        public int Id { get; set; }
    }
}
