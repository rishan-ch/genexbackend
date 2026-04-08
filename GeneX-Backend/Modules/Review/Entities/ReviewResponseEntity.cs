using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Entities;

namespace GeneX_Backend.Modules.Orders.Entities
{
    public class ReviewResponseEntity
    {
        [Key]
        public Guid ReviewResponseId { get; set; }
        public required Guid UserId { get; set; }
        public required Guid ReviewId { get; set; }
        public required string Role { get; set; }
        public required DateTime ResponseDate { get; set; }
        public required string Description { get; set; }

        public UserEntity User { get; set; }
        public ReviewEntity Review{ get; set; }
    }
}