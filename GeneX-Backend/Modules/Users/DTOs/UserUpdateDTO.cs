using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Users.DTOs
{
    public class UserUpdateDTO
    {
        public IFormFile? ProfileImage { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Address { get; set; }

        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 numeric digits")]
        public string? PhoneNumber { get; set; }
    }
}
