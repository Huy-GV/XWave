using System;
using System.ComponentModel.DataAnnotations;

namespace XWave.Models;

public class PaymentAccount : IEntity, ISoftDeletable
{
    public int Id { get; set; }
    public string AccountNumber { get; set; }
    public string Provider { get; set; }

    [DataType(DataType.Date)] public DateTime ExpiryDate { get; set; }

    public PaymentAccountDetails PaymentAccountDetails { get; set; }

    public bool IsDeleted { get; set; } = false;
    public DateTime? DeleteDate { get; set; } = null;
}