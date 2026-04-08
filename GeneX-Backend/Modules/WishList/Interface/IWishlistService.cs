using GeneX_Backend.Modules.WishList.DTOs;

namespace GeneX_Backend.Modules.WishList.Interface
{
    public interface IWishlistService
    {
        Task AddNewItem(AddWishlistDTO addWishlistDTO, Guid UserId);
        Task<List<ViewWishlistDTO>>? ViewAllWishlist(Guid UserId);
        Task RemoveItem(Guid wishListId, Guid UserId);
    }
}