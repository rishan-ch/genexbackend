using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Cart.DTOs;
using GeneX_Backend.Modules.Cart.Entities;
using GeneX_Backend.Modules.Cart.Interface;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Cart.Services
{
    public class CartService : ICartService
    {
        private readonly AppDbContext _appDbContext;

        public CartService(AppDbContext appDbCOntext)
        {
            _appDbContext = appDbCOntext;
        }

        public async Task AddProductToCart(AddToCartDTO addToCartDTO, Guid UserId)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                if (addToCartDTO.Quantity <= 0) throw new InvalidQuantityException("Please enter a valid quantity");
                //fetches product using id
                ProductEntity? fetchedProduct = await _appDbContext.Products
                    .FirstOrDefaultAsync(
                        pro => pro.ProductId == addToCartDTO.ProductId
                        &&
                        pro.DeletedAt == null
                    );
                if (fetchedProduct == null) throw new NotFoundException("Product with the id not found");

                if (fetchedProduct.ProductQuantity < addToCartDTO.Quantity) throw new InvalidQuantityException("Product doesn't have required quantity");

                //Fetch cart table and create if not existing
                CartEntity UserCart = await GetUserCart(UserId);

                //add the products to the cart table
                await AddItems(addToCartDTO, UserCart);
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //retrieve cart entity
        public async Task<CartEntity> GetUserCart(Guid UserId)
        {
            CartEntity? cart = await _appDbContext.Carts.FirstOrDefaultAsync(
                cart => cart.UserId == UserId
            );

            //create cart if not found
            if (cart == null)
            {
                await _appDbContext.Carts.AddAsync(new CartEntity()
                {
                    UserId = UserId,
                });
                await _appDbContext.SaveChangesAsync();
                cart = await _appDbContext.Carts.FirstOrDefaultAsync(
                    cart => cart.UserId == UserId
                );
            }
            return cart;
        }

        //add products to the cartItems table
        public async Task AddItems(AddToCartDTO addToCartDTO, CartEntity UserCart)
        {
            List<CartItemEntity> CartItems = await _appDbContext.CartItems.ToListAsync();
            foreach (CartItemEntity item in CartItems)
            {

                if (item.ProductId == addToCartDTO.ProductId)
                {
                    if (item.Quantity + 1 > item.Product.ProductQuantity) throw new InvalidQuantityException("Product doesn't have required quantity");

                    item.Quantity += addToCartDTO.Quantity;
                    _appDbContext.CartItems.Update(item);
                    await _appDbContext.SaveChangesAsync();
                    return;
                }
            }

            await _appDbContext.CartItems.AddAsync(new CartItemEntity()
            {
                ProductId = addToCartDTO.ProductId,
                CartId = UserCart.CartId,
                Quantity = addToCartDTO.Quantity //default
            });
            await _appDbContext.SaveChangesAsync();
        }

        //returns cart object for retrieving cart items
        //return null if cart is not created (add to cart not done by user)
        public async Task<List<ViewCartDTO>> ViewCart(Guid UserId)
        {
            CartEntity? Cart = await _appDbContext.Carts.FirstOrDefaultAsync
                (
                    cart => cart.UserId == UserId
                );

            var items = Cart == null ? new List<ViewCartDTO>() : await ViewCartItems(Cart.CartId);

            return items;
        }

        //retrieves all the items from the cartItems table
        public async Task<List<ViewCartDTO>>? ViewCartItems(Guid CartId)
        {
            List<ViewCartDTO>? CartItems = await _appDbContext.CartItems
                .Include(c => c.Product)
                    .ThenInclude(ca => ca.DiscountEntity)
                .Where(
                    item => item.CartId == CartId
                )
                .Select(i =>
                    new ViewCartDTO()
                    {
                        CartItemId = i.CartItemId,
                        ProductId = i.ProductId,
                        ProductName = i.Product.ProductName,
                        Quantity = i.Quantity,
                        Price = i.Product.ProductUnitPrice,
                        PriceAfterDiscount = i.Product.DiscountEntity != null
                            && i.Product.DiscountEntity.DiscountPercentage > 0
                                ? i.Product.ProductUnitPrice - (i.Product.ProductUnitPrice * i.Product.DiscountEntity.DiscountPercentage / 100)
                                : i.Product.ProductUnitPrice,
                        ImageUrl = i.Product.ProductImages
                            .Select(img => img.ProductImageUrl)
                            .FirstOrDefault(),
                    }
                )
                .ToListAsync();

            return CartItems;
        }

        //manipulate the quantity of a product its id
        public async Task ChangeQuantity(ChangeQtyDTO changeQtyDTO)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                CartItemEntity? CartItem = await _appDbContext.CartItems.FirstOrDefaultAsync
                (
                    item => item.CartItemId == changeQtyDTO.CartItemId
                );
                if (CartItem == null) throw new NotFoundException("Cart item not found");

                CartItem.Quantity = (int)(CartItem.Quantity + changeQtyDTO.Quantity);

                _appDbContext.CartItems.Update(CartItem);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }


        }

        public async Task DeleteCartItem(DelItemDTO delItemDTO)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                CartItemEntity? CartItem = await _appDbContext.CartItems.FirstOrDefaultAsync
                (
                    item => item.CartItemId == delItemDTO.CartItemId
                );

                if (CartItem == null) throw new NotFoundException("Cart item not found");

                _appDbContext.CartItems.Remove(CartItem);
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        public async Task DeleteAllItems(Guid UserID)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {
                List<CartItemEntity> cartItems = await _appDbContext.CartItems
                    .Where(
                        item => item.Cart.UserId == UserID
                    ).ToListAsync();

                foreach (CartItemEntity item in cartItems)
                {
                    _appDbContext.CartItems.Remove(item);
                }
                await _appDbContext.SaveChangesAsync();
                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

    }
}