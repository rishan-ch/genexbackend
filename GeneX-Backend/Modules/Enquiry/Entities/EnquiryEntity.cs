using GeneX_Backend.Modules.Products.Entities;
using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Enquiry.Entities
{
    public class EnquiryEntity
    {
        [Key]
        public required Guid EnquiryId { get; set; }
        public required string PersonName { get; set; }
        public required string PersonEmail { get; set; }
        public required int PersonPhoneNumber { get; set; }
        public required string EnquiryDescription { get; set; }
        public required Guid RelatedProductId { get; set; }
        public ProductEntity Product { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    }
}
