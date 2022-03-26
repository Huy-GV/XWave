using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Product : IEntity, ISoftDeletable
    {
        // todo: change to string and allow manual input
        public int Id { get; set; }
        [Required]
        [StringLength(15, MinimumLength = 2)]
        public string Name { get; set; }
        // todo: add description and country of manufacture
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        //public string ImagePath { get; set; }
        [Range(0, double.MaxValue, ErrorMessage ="Quantity cannot be negative")]
        public uint Quantity { get; set; }
        public DateTime? LatestRestock { get; set; }
        public bool IsDiscontinued { get; set; }
        [DataType("datetime")]
        public DateTime? DiscontinuationDate { get; set; }
        public int CategoryId { get; set; }
        public int? DiscountId { get; set; }
        public Category Category { get; set; }
        [ForeignKey("DiscountId")]
        public Discount? Discount { get; set; }
        public bool IsDeleted { get; set; } = false;
        public DateTime? DeleteDate { get; set; } = null;
    }
}