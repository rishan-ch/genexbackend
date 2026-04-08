using GeneX_Backend.Modules.Enquiry.DTOs;
using GeneX_Backend.Modules.Enquiry.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{

    [ApiController]
    [Route("api/enquiry")]
    public class EnquiryController : ControllerBase
    {
        private readonly IEnquiryService _enquiryService;

        public EnquiryController(IEnquiryService enquiryService)
        {
            _enquiryService = enquiryService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateEnquiry([FromBody] AddEnquiryDto dto)
        {
            try
            {
                var result = await _enquiryService.AddEnquiryAsync(dto);
                if (result == null)
                {
                    return BadRequest(new { Success = false, Message = "You can only submit an enquiry once every 12 hours." });
                }
                return Ok(new { Success = true, Data = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }

        [HttpGet]
        public async Task<IActionResult> GetAllEnquiries()
        {
            try
            {
                var enquiries = await _enquiryService.GetAllEnquiriesAsync();
                return Ok(new { Success = true, Data = enquiries });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEnquiry(Guid id)
        {
            try
            {
                var result = await _enquiryService.DeleteEnquiryAsync(id);
                return Ok(new { Success = result, Message = "Enquiry deleted successfully." });
            }
            catch (NotFoundException Ex)
            {
                return NotFound(new { Success = false, Message = Ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { Success = false, Message = ex.Message });
            }
        }
    }
}
