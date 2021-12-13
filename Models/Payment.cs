using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Payment : IValidatableObject
    {
        public int ID { get; set; }
        public uint AccountNo { get; set; }
        public string Provider { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
        [NotMapped]
        public readonly string[] ValidProviders = new string[]
        {
            "mastercard",
            "americanexpress",
            "visa"
        };
        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (!Array.Exists(ValidProviders, element => element == Provider))
            {
                yield return new ValidationResult(
                    "Only MasterCard, AmericanExpress, and Visa are valid providers",
                    new string[] { nameof(Provider) });
            }

            if (ExpiryDate <= DateTime.Now)
            {
                yield return new ValidationResult(
                    "Account expired on the date of purchase",
                    new string[] { nameof(ExpiryDate) });
            }
        }
    }
}
