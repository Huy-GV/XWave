using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Customer : IEntity
    {
        public string CustomerId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string BillingAddress { get; set; }
    }
}
