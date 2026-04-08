using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Banner
{
    public class ViewBannerDTO
    {
        public required Guid BannerId { get; set; }
        public string? Hyperlink { get; set; }
        public required string ImageUrl { get; set; }
    }
}