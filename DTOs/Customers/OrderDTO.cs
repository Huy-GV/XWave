using System.Collections.Generic;
using System;
using XWave.Models;

namespace XWave.DTOs.Customers
{
    public class OrderDTO
    {
        public  IEnumerable<OrderDetailDTO> OrderDetailCollection { get; set; }   
        public DateTime OrderDate { get; set; }
        public uint AccountNo { get; set; }
        public int ID { get; set; }
    }
}
