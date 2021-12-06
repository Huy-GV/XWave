﻿using System.ComponentModel.DataAnnotations;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.Models
{
    public class Payment : IValidatableObject
    {
        public int ID { get; set; }
        [Required]
        public string Provider { get; set; }
        [Required]
        [DataType(DataType.Date)]
        public DateTime ExpiryDate { get; set; }
        [Required]
        [Range(4,20)]
        public int AccountNumber { get; set; }
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
        }
    }
}