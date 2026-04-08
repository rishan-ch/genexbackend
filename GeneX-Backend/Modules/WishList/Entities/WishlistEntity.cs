using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Entities;

namespace GeneX_Backend.Modules.WishList.Entities
{
    public class WishlistEntity
    {
        [Key]
        public Guid WishlistId { get; set; }
        public required Guid UserId { get; set; }
        public required Guid ProductId { get; set; }

        public UserEntity User { get; set; }
        public ProductEntity Product{ get; set; }
    }
}