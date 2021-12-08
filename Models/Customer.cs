using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Customer
    {
        //public int ID { get; set; }
        [Key]
        public string CustomerID { get; set; }
        //public string Email { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Country { get; set; }
        [Column(TypeName= "int")]
        [Range(0, int.MaxValue)]
        public int PhoneNumber { get; set; }
        [StringLength(50, MinimumLength = 5)]
        public string Address { get; set; }
    }
}
