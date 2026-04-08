using GeneX_Backend.Modules.Banner;
using GeneX_Backend.Modules.Banner.Interface;
using GeneX_Backend.Shared.Enums;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/banner")]
    public class BannerController : ControllerBase
    {

        private readonly IBannerService _bannerService;

        public BannerController(IBannerService bannerService)
        {
            _bannerService = bannerService;
        }

        [HttpPost]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> CreateBanner([FromForm] AddBannerDTO addbannerDTO)
        {
            try
            {

                await _bannerService.AddBanner(addbannerDTO);

                return Ok(new { success = true, message = "A new banner has been added" });

            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("all")]
        public async Task<IActionResult> ShowAllBanners()
        {
            try
            {
                List<ViewBannerDTO>? bannerList = await _bannerService.GetAllBanner();

                return Ok(new { success = true, message = bannerList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("genex")]
        public async Task<IActionResult> ShowAllGenex()
        {
            try
            {
                List<ViewBannerDTO>? bannerList = await _bannerService.GetCategoryBanner(BannerCategory.GeneX);

                return Ok(new { success = true, message = bannerList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet("chunlan")]
        public async Task<IActionResult> ShowAllChunlan()
        {
            try
            {
                List<ViewBannerDTO>? bannerList = await _bannerService.GetCategoryBanner(BannerCategory.Chunlan);

                return Ok(new { success = true, message = bannerList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }
        [HttpGet("polytron")]
        public async Task<IActionResult> ShowAllPolytron()
        {
            try
            {
                List<ViewBannerDTO>? bannerList = await _bannerService.GetCategoryBanner(BannerCategory.Polytron);

                return Ok(new { success = true, message = bannerList });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpDelete("{bannerId}")]
        [Authorize(Roles = "SuperAdmin")]
        public async Task<IActionResult> Deletebanner([FromRoute] Guid bannerId)
        {
            try
            {
                await _bannerService.RemoveBanner(bannerId);

                return Ok(new { success = true, message = "The banner has been removed" });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { success = false, message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

    }
}
