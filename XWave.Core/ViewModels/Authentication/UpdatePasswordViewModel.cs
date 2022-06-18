using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Authentication;

public class UpdatePasswordViewModel
{
    [Required]
    [DataType(DataType.Password)]
    public string CurrentPassword { get; set; }

    [DataType(DataType.Password)] public string NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(NewPassword), ErrorMessage = "Password must match")]
    public string ConfirmPassword { get; set; }
}