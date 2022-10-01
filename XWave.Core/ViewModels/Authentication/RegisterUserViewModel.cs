using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Authentication;

public class RegisterUserViewModel
{
    [Required] public string FirstName { get; set; } = string.Empty;

    [Required] public string LastName { get; set; } = string.Empty;

    [Required] public string UserName { get; set; } = string.Empty;

    //[Required]
    //public string Email { get; set; }
    [Required] public string Password { get; set; } = string.Empty;
}