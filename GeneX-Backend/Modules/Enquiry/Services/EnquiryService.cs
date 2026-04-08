using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Enquiry.DTOs;
using GeneX_Backend.Modules.Enquiry.Entities;
using GeneX_Backend.Modules.Enquiry.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Enquiry.Services
{
    public class EnquiryService : IEnquiryService
    {
        private readonly AppDbContext _appDbContext;

        public EnquiryService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        //Create new inquiry
        public async Task<EnquiryEntity> AddEnquiryAsync(AddEnquiryDto dto)
        {
            var twelveHoursAgo = DateTime.UtcNow.AddHours(-12);

            // Check if enquiry exists from the same email or phone in last 12 hours
            bool hasRecentEnquiry = await _appDbContext.Enquiries
                .AnyAsync(e =>
                    (e.PersonEmail == dto.PersonEmail || e.PersonPhoneNumber == dto.PersonPhoneNumber) &&
                    e.CreatedAt >= twelveHoursAgo);

            if (hasRecentEnquiry)
            {
                return null; // Or throw a custom exception to handle in controller
            }

            var enquiry = new EnquiryEntity
            {
                EnquiryId = Guid.NewGuid(),
                PersonName = dto.PersonName,
                PersonEmail = dto.PersonEmail,
                PersonPhoneNumber = dto.PersonPhoneNumber,
                EnquiryDescription = dto.EnquiryDescription,
                RelatedProductId = dto.RelatedProductId,
                CreatedAt = DateTime.UtcNow
            };

            await _appDbContext.Enquiries.AddAsync(enquiry);
            await _appDbContext.SaveChangesAsync();
            return enquiry;
        }

        //get all the inquiries
        public async Task<List<ViewEnquiryDto>> GetAllEnquiriesAsync()
        {
            return await _appDbContext.Enquiries
                .Include(e => e.Product)
                .Select(e => new ViewEnquiryDto
                {
                    EnquiryId = e.EnquiryId,
                    PersonName = e.PersonName,
                    PersonEmail = e.PersonEmail,
                    PersonPhoneNumber = e.PersonPhoneNumber,
                    EnquiryDescription = e.EnquiryDescription,
                    RelatedProductId = e.RelatedProductId,
                    Product = e.Product == null ? null : new Products.DTOs.ViewProductDTO
                    {
                        ProductId = e.Product.ProductId,
                        ProductName = e.Product.ProductName
                    }


                })
                .ToListAsync();
        }

        //Delete the inquiry
        public async Task<bool> DeleteEnquiryAsync(Guid id)
        {
            var enquiry = await _appDbContext.Enquiries.FindAsync(id);

            if (enquiry == null)
            {
                throw new NotFoundException("Inquiry not found");
            }

            _appDbContext.Enquiries.Remove(enquiry);
            await _appDbContext.SaveChangesAsync();
            return true;
        }
    }
}
