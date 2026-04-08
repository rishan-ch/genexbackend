using GeneX_Backend.Modules.Dashbaord;
using Microsoft.AspNetCore.Mvc;

namespace GeneX_Backend.Controllers
{
    [ApiController]
    [Route("/api/dashboard")]
    public class DashboardController : ControllerBase
    {

        private readonly DashboardService _dashboardService;

        public DashboardController(DashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("stats")]
        public async Task<IActionResult> DashboardDetails()
        {
            try
            {
                var result = await _dashboardService.GetDashboardDetails();

                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }


        [HttpGet("recentOrders")]
        public async Task<IActionResult> RecentOrdersData()
        {
            try
            {
                var result = await _dashboardService.Recent5Orders();

                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("products")]
        public async Task<IActionResult> ProductsData()
        {
            try
            {
                var result = await _dashboardService.GetTop5MostSoldProducts();

                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }

        [HttpGet("monthlySales")]
        public async Task<IActionResult> SalesByMonth([FromQuery] string from, [FromQuery] string to)
        {
            try
            {
                var result = await _dashboardService.SalesByMonth(from, to);
                return Ok(new { success = true, message = result });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = ex.Message });
            }
        }



    }
}
