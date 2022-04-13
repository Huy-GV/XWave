using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Product : IEntity, ISoftDeletable
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(30)]
        public string Name { get; set; }

        [Required]
        [MaxLength(100)]
        public string Description { get; set; }

        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }

        //public string ImagePath { get; set; }
        [Range(0, int.MaxValue)]
        public uint Quantity { get; set; }

        public DateTime? LatestRestock { get; set; } = null;
        public bool IsDiscontinued { get; set; } = false;

        [DataType("datetime")]
        public DateTime? DiscontinuationDate { get; set; } = null;

        public int CategoryId { get; set; }
        public int? DiscountId { get; set; }
        public Category Category { get; set; }

        [ForeignKey("DiscountId")]
        public Discount? Discount { get; set; } = null;

        public bool IsDeleted { get; set; } = false;
        public DateTime? DeleteDate { get; set; } = null;
    }
}