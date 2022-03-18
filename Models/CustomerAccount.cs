using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class CustomerAccount : IEntity
    {
        [Key]
        public string CustomerId { get; set; }
        [Required]
        [StringLength(100, MinimumLength = 1)]
        public string BillingAddress { get; set; } = string.Empty;
        //public bool IsSubscribedToPromotions { get; set; } = false;
    }
}
