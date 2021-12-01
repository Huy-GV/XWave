using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class Product
    {
        public int ID { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        public int Price { get; set; }
        public string ImagePath { get; set; }

        [Range(0, double.MaxValue, ErrorMessage ="Quantity cannot be negative")]
        public int Quantity { get; set; }
        public DateTime LastRestock { get; set; }
        public int? CategoryID { get; set; }
        public int? DiscountID { get; set; }
        //navigation properties
        #nullable enable
        public Category? Category { get; set; }
        public Discount? Discount { get; set; }
    }
}