

using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.WishList.DTOs;
using GeneX_Backend.Modules.WishList.Entities;
using GeneX_Backend.Modules.WishList.Interface;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.WishList.Service
{
    public class WishlistService : IWishlistService
    {

        private readonly AppDbContext _appDbContext;

        public WishlistService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddNewItem(AddWishlistDTO addWishlistDTO, Guid UserId)
        {
            await _appDbContext.Wishlists.AddAsync(
                new WishlistEntity
                {
                    WishlistId = Guid.NewGuid(),
                    ProductId = addWishlistDTO.ProductId,
                    UserId = UserId
                }
            );

            await _appDbContext.SaveChangesAsync();
        }

        public async Task RemoveItem(Guid wishListId, Guid UserId)
        {
            WishlistEntity? wishList = await _appDbContext.Wishlists.FirstOrDefaultAsync(
                w => w.WishlistId == wishListId
                && w.UserId == UserId
            );

            if (wishList == null)
                throw new NotFoundException("The wishlist item doesn't exist in the system");

            _appDbContext.Wishlists.Remove(wishList);
            await _appDbContext.SaveChangesAsync();
        }

        public async Task<List<ViewWishlistDTO>>? ViewAllWishlist(Guid UserId)
        {
            List<ViewWishlistDTO>? wishlists = await _appDbContext.Wishlists
                .Include(w => w.Product).ThenInclude(p => p.ProductImages)
                .Where(
                    w => w.UserId == UserId &&
                    !w.Product.IsDeleted
                )
                .Select(
                    wi => new ViewWishlistDTO
                    {
                        WishlistId = wi.WishlistId,
                        ProductId = wi.ProductId,
                        ImageUrl = wi.Product.ProductImages
                            .Select(img => img.ProductImageUrl)
                            .FirstOrDefault(),
                        ProductName = wi.Product.ProductName,
                        ProductPrice = wi.Product.ProductUnitPrice
                    }
                )
                .ToListAsync();

            return wishlists;
        }
    }
}