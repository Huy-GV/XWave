using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System;

namespace XWave.Models
{
    public class Discount
    {
        public int ID { get; set; }
        [Required]
        [Range(1, 100, ErrorMessage ="Discount percentage outside valid range")]
        public int Percentage { get; set; }
        public bool IsActive { get; set; } = false;
        [Required]
        public DateTime StartDate { get; set; }
        [Required]
        public DateTime EndDate { get; set; } 
    }
}