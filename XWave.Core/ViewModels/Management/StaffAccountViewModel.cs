﻿using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Management;

public class StaffAccountViewModel : IValidatableObject
{
    [Range(20, 500)] 
    public uint HourlyWage { get; set; }
    public DateTime ContractEndDate { get; set; }

    public string ImmediateManagerId { get; set; } = string.Empty;

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (ContractEndDate > DateTime.Now.AddDays(30))
        {
            yield return new ValidationResult("Contract must last at least 30 days.");
        }  
    }
}