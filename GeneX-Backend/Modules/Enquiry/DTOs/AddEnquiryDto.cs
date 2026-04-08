using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Enquiry.DTOs
{
    public class AddEnquiryDto
    {
        [Required(ErrorMessage = "Person name is required.")]
        [StringLength(100, ErrorMessage = "Name cannot exceed 100 characters.")]
        public required string PersonName { get; set; }

        [Required(ErrorMessage = "Email is required.")]
        [EmailAddress(ErrorMessage = "Invalid email address.")]
        public required string PersonEmail { get; set; }

        [Required(ErrorMessage = "Phone number is required.")]
        [Range(1000000000, 9999999999, ErrorMessage = "Phone number must be 10 digits.")]
        public required int PersonPhoneNumber { get; set; }

        [Required(ErrorMessage = "Enquiry description is required.")]
        [StringLength(500, ErrorMessage = "Description cannot exceed 500 characters.")]
        public required string EnquiryDescription { get; set; }

        [Required(ErrorMessage = "Related product ID is required.")]
        public required Guid RelatedProductId { get; set; }
    }
}
