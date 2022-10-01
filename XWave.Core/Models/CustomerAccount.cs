using System.ComponentModel.DataAnnotations;

namespace XWave.Core.Models;

public class CustomerAccount : IEntity
{
    [Key] 
    public string CustomerId { get; set; }

    [Required] 
    [MaxLength(100)] 
    public string BillingAddress { get; set; }

    public bool IsSubscribedToPromotions { get; set; } = false;
}