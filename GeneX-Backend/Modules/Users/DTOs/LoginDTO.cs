using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Users.DTOs
{
    public class UserLoginDTO
    {
        [Required(ErrorMessage = "Email Address is not defined")]
        [EmailAddress]
        public string? Email { get; set; }

        [Required]
        public string? Password { get; set; }
    }
}
