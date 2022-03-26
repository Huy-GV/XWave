﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;


namespace XWave.ViewModels.Management
{
    public class ProductViewModel
    {
        [Required]
        [StringLength(15, MinimumLength = 2)]
        public string Name { get; set; }
        [Required]
        [Column(TypeName = "decimal(18,4)")]
        public decimal Price { get; set; }
        //TODO: implement image path
        //public string ImagePath { get; set; }

        //[Range(0, double.MaxValue, ErrorMessage = "Quantity cannot be negative")]
        //public uint Quantity { get; set; }
        public int CategoryId { get; set; }
    }
}
