using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace XWave.Models
{
    public class Discount : IEntity
    {
        public int ID { get; set; }
        [Required]
        [Range(1, 100, ErrorMessage ="Discount percentage outside valid range")]
        public uint Percentage { get; set; }
        public bool IsActive 
        { 
            get 
            {
                var currentDate = DateTime.Now;
                return currentDate > StartDate && currentDate < EndDate;
            }   
        } 
        [Required]
        [Column(TypeName ="date")]
        public DateTime StartDate { get; set; }
        [Required]
        [Column(TypeName = "date")]
        public DateTime EndDate { get; set; }
        public string ManagerID { get; set; }
        public ApplicationUser Manager { get; set; }
        public ICollection<Product> Products { get; set; }
    }
}