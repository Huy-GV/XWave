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
        public int InventoryID { get; set; }
        public int? CategoryID { get; set; }
        public int? DiscountID { get; set; }
        //navigation properties
        public Inventory Inventory { get; set; }
        #nullable enable
        public Category? Category { get; set; }
        public Discount? Discount { get; set; }
    }
}