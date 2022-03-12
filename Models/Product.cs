using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Product : IEntity
    {
        // change to string and allow manual input
        public int Id { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        //public string ImagePath { get; set; }

        [Range(0, double.MaxValue, ErrorMessage ="Quantity cannot be negative")]
        public uint Quantity { get; set; }
        public DateTime LastRestock { get; set; }
        public int CategoryId { get; set; }
        public int? DiscountId { get; set; }
        //navigation properties
        public Category Category { get; set; }
        [ForeignKey("DiscountId")]
        public Discount? Discount { get; set; }
    }
}