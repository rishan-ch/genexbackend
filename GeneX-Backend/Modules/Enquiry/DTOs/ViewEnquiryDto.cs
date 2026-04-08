using GeneX_Backend.Modules.Products.DTOs;

namespace GeneX_Backend.Modules.Enquiry.DTOs
{
    public class ViewEnquiryDto
    {
        public Guid EnquiryId { get; set; }
        public string PersonName { get; set; }
        public string PersonEmail { get; set; }
        public int PersonPhoneNumber { get; set; }
        public string EnquiryDescription { get; set; }
        public Guid RelatedProductId { get; set; }

   
        public ViewProductDTO? Product { get; set; }
    }
}
