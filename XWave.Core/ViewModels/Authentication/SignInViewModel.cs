using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Authentication;

public class SignInViewModel
{
    [Required] 
    public string Username { get; set; } = string.Empty;

    [Required] 
    [DataType(DataType.Password)]
    public string Password { get; set; } = string.Empty;
}