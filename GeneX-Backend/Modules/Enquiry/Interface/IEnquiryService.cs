using GeneX_Backend.Modules.Enquiry.DTOs;
using GeneX_Backend.Modules.Enquiry.Entities;

namespace GeneX_Backend.Modules.Enquiry.Interface
{
    public interface IEnquiryService
    {
        Task<EnquiryEntity?> AddEnquiryAsync(AddEnquiryDto dto);
        Task<List<ViewEnquiryDto>> GetAllEnquiriesAsync();
        Task<bool> DeleteEnquiryAsync(Guid id);
    }
}
