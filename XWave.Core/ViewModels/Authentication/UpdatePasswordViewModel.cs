using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Authentication;

public class UpdatePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)] 
    public string NewPassword { get; set; } = string.Empty;

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Password must match")]
    public string ConfirmPassword { get; set; } = string.Empty;
}