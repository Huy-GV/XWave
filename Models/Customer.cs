using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Customer
    {
        [Key]
        public string CustomerID { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Country { get; set; }
    }
}
