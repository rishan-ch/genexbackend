using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Banner
{
    public class AddBannerDTO
    {
        public string? Hyperlink { get; set; }
        public required IFormFile ImageFIle { get; set; }
        public required BannerCategory BannerCategory{ get; set; }
    }
}