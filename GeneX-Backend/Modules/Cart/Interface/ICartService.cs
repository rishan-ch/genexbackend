using GeneX_Backend.Modules.Cart.DTOs;
using GeneX_Backend.Modules.Cart.Entities;

namespace GeneX_Backend.Modules.Cart.Interface
{
    public interface ICartService
    {
        Task AddProductToCart(AddToCartDTO addToCartDTO, Guid UserId);
        Task<CartEntity> GetUserCart(Guid UserId);
        Task AddItems(AddToCartDTO addToCartDTO, CartEntity UserCart);
        Task<List<ViewCartDTO>> ViewCart(Guid UserId);
        Task ChangeQuantity(ChangeQtyDTO changeQtyDTO);
        Task<List<ViewCartDTO>> ViewCartItems(Guid CartId);
        Task DeleteCartItem(DelItemDTO delItemDTO);
        Task DeleteAllItems(Guid UserId);
    }
}