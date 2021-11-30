using System.ComponentModel.DataAnnotations;

namespace XWave.Data.RequestModels
{
    public class LogInModel
    {
        [Required]
        public string Username { get; set; }
        [Required]
        public string Password { get; set; }
    }
}