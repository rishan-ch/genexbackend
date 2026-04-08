using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Modules.Users.Models;

namespace GeneX_Backend.Modules.Cart.Entities
{
    public class CartEntity
    {
        [Key]
        public Guid CartId { get; set; }
        public required Guid UserId { get; set; }
        public UserEntity User;
        public ICollection<CartItemEntity>? Items{ get; set; }

    }

    public class CartItemEntity
    {
        [Key]
        public Guid CartItemId { get; set; }
        public required Guid ProductId { get; set; }
        public required Guid CartId { get; set; }
        public required int Quantity { get; set; }

        public ProductEntity Product { get; set; }
        public CartEntity Cart{ get; set; }
        
    }
}