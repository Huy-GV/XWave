using System.ComponentModel.DataAnnotations;

namespace XWave.ViewModels.Authentication
{
    public class SignInViewModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}