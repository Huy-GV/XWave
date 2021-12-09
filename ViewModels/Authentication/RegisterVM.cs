using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace XWave.ViewModels.Authentication
{
    public class RegisterVM
    {
        [Required]
        public string FirstName { get; set; }
        [Required]
        public string LastName { get; set; }
        [Required]
        public string Username { get; set; }
        //[Required]
        //public string Email { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        [StringLength(20, MinimumLength = 2)]
        public string Country { get; set; }
        [Column(TypeName = "int")]
        [Range(0, int.MaxValue)]
        public int PhoneNumber { get; set; }
        [StringLength(50, MinimumLength = 5)]
        public string Address { get; set; }
    }
}