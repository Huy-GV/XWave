using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Core.Models;

public class PaymentAccountDetails : IEntity
{
    public string CustomerId { get; set; }
    public int PaymentAccountId { get; set; }

    [Column(TypeName = "datetime")] public DateTime FirstRegistration { get; set; }

    public CustomerAccount Customer { get; set; }
    public PaymentAccount Payment { get; set; }
}