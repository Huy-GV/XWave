using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models
{
    public class Customer
    {
        [Required]
        public int ID { get; set; }
        //public string Email { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Country { get; set; }
        public int? PaymentID { get; set; }
        public Payment? Payment { get; set; }
    }
}
