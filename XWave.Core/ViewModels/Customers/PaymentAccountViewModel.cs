using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Customers;

public class PaymentAccountViewModel : IValidatableObject
{
    private readonly string[] ValidProviders =
    {
        "mastercard",
        "americanexpress",
        "visa"
    };

    [Required] public string AccountNumber { get; set; } = string.Empty;

    [Required] public string Provider { get; set; } = string.Empty;

    [Required] [DataType(DataType.Date)] public DateTime ExpiryDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (!Array.Exists(ValidProviders, element => element == Provider))
        {
            yield return new ValidationResult(
                "Only MasterCard, AmericanExpress, and Visa are valid providers",
                new[] { nameof(Provider) });
        }

        if (ExpiryDate <= DateTime.Now)
        {
            yield return new ValidationResult(
                "Account expired on the date of purchase",
                new[] { nameof(ExpiryDate) });
        }
    }
}