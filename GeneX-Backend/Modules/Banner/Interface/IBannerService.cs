using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Banner.Interface
{
    public interface IBannerService
    {
        Task AddBanner(AddBannerDTO addBannerDTO);
        Task<List<ViewBannerDTO>> GetAllBanner();
        Task<List<ViewBannerDTO>> GetCategoryBanner(BannerCategory? bannerCategory);
        Task RemoveBanner(Guid BannerId);
    }
}