using GeneX_Backend.Infrastructure.CloudinaryService;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Banner.Entity;
using GeneX_Backend.Modules.Banner.Interface;
using GeneX_Backend.Shared.Enums;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Banner.Service
{
    public class BannerService : IBannerService
    {

        private readonly AppDbContext _appDbContext;
        private readonly CloudinaryService _cloudinaryService;

        public BannerService(AppDbContext appDbContext, CloudinaryService cloudinaryService)
        {
            _appDbContext = appDbContext;
            _cloudinaryService = cloudinaryService;
        }

        public async Task AddBanner(AddBannerDTO addBannerDTO)
        {
            string imageUrl = await UploadToCloudinary(addBannerDTO.ImageFIle);

            await _appDbContext.Banners.AddAsync(
                new BannerEntity
                {
                    BannerId = Guid.NewGuid(),
                    Hyperlink = addBannerDTO.Hyperlink,
                    ImageUrl = imageUrl,
                    BannerCategory = addBannerDTO.BannerCategory
                }
            );

            await _appDbContext.SaveChangesAsync();
        }

        public async Task<string> UploadToCloudinary(IFormFile bannerPic)
        {
            string imageUrl = await _cloudinaryService.UploadImageAsync(bannerPic);
            return imageUrl;
        }

        public async Task<List<ViewBannerDTO>> GetAllBanner()
        {
            var query = _appDbContext.Banners.AsQueryable();

            List<ViewBannerDTO> bannerList = await query
                .Where(b => b.BannerCategory == BannerCategory.All)
                .Select(b => new ViewBannerDTO
                {
                    BannerId = b.BannerId,
                    Hyperlink = b.Hyperlink,
                    ImageUrl = b.ImageUrl
                })
                .ToListAsync();

            return bannerList;
        }

        public async Task<List<ViewBannerDTO>> GetCategoryBanner(BannerCategory? bannerCategory)
        {
            var query = _appDbContext.Banners.AsQueryable();

            if (bannerCategory.HasValue && bannerCategory != BannerCategory.All)
            {
                query = query.Where(b => b.BannerCategory == bannerCategory);
            }

            List<ViewBannerDTO> bannerList = await query
                .Select(b => new ViewBannerDTO
                {
                    BannerId = b.BannerId,
                    Hyperlink = b.Hyperlink,
                    ImageUrl = b.ImageUrl
                })
                .ToListAsync();

            return bannerList;
        }



        public async Task RemoveBanner(Guid BannerId)
        {
            BannerEntity? banner = await _appDbContext.Banners.FirstOrDefaultAsync(b => b.BannerId == BannerId);

            if (banner == null)
                throw new NotFoundException("The required banner doesn't exist in the system");

            _appDbContext.Banners.Remove(banner);
            await _appDbContext.SaveChangesAsync();
        }
    }
}
