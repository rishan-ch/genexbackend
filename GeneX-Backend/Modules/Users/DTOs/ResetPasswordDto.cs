using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Users.DTOs
{
    public class ResetPasswordDto
    {
        public required string Data { get; set; }
        [Required]
        [RegularExpression(
            @"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[^A-Za-z\d]).{8,}$",
            ErrorMessage = "Password must be at least 8 characters long, include uppercase, lowercase, number, and special character"
        )]
        public required string NewPassword { get; set; }
        [Compare(nameof(NewPassword), ErrorMessage = "Passwords don't match")]
        public required string ConfirmPassword { get; set; }
    }
}
