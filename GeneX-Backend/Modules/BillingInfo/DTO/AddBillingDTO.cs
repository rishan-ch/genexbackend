using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.BillingInfo.DTO
{
    public class AddBillingDTO
    {
        public required string FullName { get; set; }
        
        [Required(ErrorMessage = "Phone Number is missing")]
        [StringLength(10, MinimumLength = 10, ErrorMessage = "Phone number must be exactly 10 digits")]
        [RegularExpression(@"^\d{10}$", ErrorMessage = "Phone number must be exactly 10 numeric digits")]
        public required string PhoneNumber { get; set; }
        public required string Province { get; set; }
        public required string City { get; set; }
        public required string Address { get; set; }
        public string? LandMark { get; set; }
        public required BillingLabel Label { get; set; }
    }
}