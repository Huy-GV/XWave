using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    //TODO: add another ID field to be set by the user
    public class Product : IEntity
    {
        public int ID { get; set; }
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
        public int CategoryID { get; set; }
        public int? DiscountID { get; set; }
        //navigation properties
        public Category Category { get; set; }
#nullable enable
        [ForeignKey("DiscountID")]
        public Discount? Discount { get; set; }
    }
}