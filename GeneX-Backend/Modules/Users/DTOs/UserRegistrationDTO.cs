using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Users.DTOs
{
    public class UserRegistrationDTO

    {
        public IFormFile? ProfileImage { get; set; }

        [Required(ErrorMessage = "Firstname is not defined")]
        public string? Firstname { get; set; }

        public string? Lastname { get; set; }

        [Required(ErrorMessage = "Address is not defined")]
        public string? Address { get; set; }

        [Required(ErrorMessage = "Email is not defined")]
        [EmailAddress]
        public required string Email { get; set; }

        [Required(ErrorMessage = "Phone Number is missing")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 numeric digits")]
        public string? PhoneNumber { get; set; }

        [Required]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, include uppercase, lowercase, number, and special character"
        )]
        public string? Password { get; set; }

        [Compare(nameof(Password), ErrorMessage = "Passwords don't match")]
        [Required]
        public string? ConfirmPassword { get; set; }

    }
}
