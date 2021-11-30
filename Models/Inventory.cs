using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class Inventory
    {
        public int ID { get; set; }
        [Range(0, double.MaxValue, ErrorMessage ="Quantity cannot be negative")]
        public int Quantity { get; set; }
        public DateTime LastRestock { get; set; }
    }
}