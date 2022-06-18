using System.ComponentModel.DataAnnotations;

namespace XWave.Core.ViewModels.Authentication;

public class RegisterUserViewModel
{
    [Required] public string FirstName { get; set; }

    [Required] public string LastName { get; set; }

    [Required] public string Username { get; set; }

    //[Required]
    //public string Email { get; set; }
    [Required] public string Password { get; set; }
}