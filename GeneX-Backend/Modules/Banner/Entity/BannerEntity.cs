using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Banner.Entity
{
    public class BannerEntity
    {
        [Key]
        public Guid BannerId { get; set; }
        public string? Hyperlink { get; set; }
        public required BannerCategory BannerCategory{ get; set; }
        public required string ImageUrl { get; set; }
    }
}