using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace XWave.ViewModels.Management;

public class DiscountViewModel
{
    [Required]
    [Range(1, 100, ErrorMessage = "Discount percentage outside valid range")]
    public uint Percentage { get; set; }

    [Required] public DateTime StartDate { get; set; }

    [Required] public DateTime EndDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext _)
    {
        if (StartDate > EndDate)
            yield return new ValidationResult(
                "Start date must be before end date.",
                new[] { nameof(StartDate), nameof(EndDate) });
    }
}