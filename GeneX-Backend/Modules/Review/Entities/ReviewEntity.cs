using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Entities;

namespace GeneX_Backend.Modules.Orders.Entities
{
    public class ReviewEntity
    {
        [Key]
        public Guid ReviewId { get; set; }
        public required Guid ProductId { get; set; }
        public required Guid UserId { get; set; }
        public string? Description { get; set; }
        public required DateTime ReviewDate{ get; set; }
        public required int StarCount { get; set; }

        public UserEntity User { get; set; }
        public ProductEntity Product{ get; set; }
    }
}