using System.ComponentModel.DataAnnotations;

namespace XWave.Core.Models;

public class PaymentAccount : IEntity, ISoftDeletable
{
    public int Id { get; set; }
    public string AccountNumber { get; set; } = string.Empty;

    public string Provider { get; set; } = string.Empty;

    [DataType(DataType.Date)] 
    public DateTime ExpiryDate { get; set; }

    public PaymentAccountDetails PaymentAccountDetails { get; set; } = null!;

    public bool IsDeleted { get; set; } = false;

    public DateTime? DeleteDate { get; set; } = null;
}