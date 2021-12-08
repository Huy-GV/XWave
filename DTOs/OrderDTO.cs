using System.Collections.Generic;
using System;
using XWave.Models;

namespace XWave.DTOs
{
    public class OrderDTO
    {
        public  ICollection<OrderDetail> OrderDetailCollection { get; set; }   
        public DateTime OrderDate { get; set; }
        public uint AccountNo { get; set; }
        
    }
}
