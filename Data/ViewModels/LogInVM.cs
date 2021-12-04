using System.ComponentModel.DataAnnotations;

namespace XWave.Data.ViewModels
{
    public class LogInVM
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}