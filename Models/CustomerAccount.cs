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
        [MaxLength(100)]
        public string BillingAddress { get; set; } = string.Empty;
        public bool IsSubscribedToPromotions { get; set; } = false;
    }
}
