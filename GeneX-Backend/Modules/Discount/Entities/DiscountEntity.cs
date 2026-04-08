using GeneX_Backend.Modules.Category.Entities;
using GeneX_Backend.Modules.Products.Entities;
using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Discount.Entities
{
    public class DiscountEntity
    {
        [Key]
        public Guid DiscountId { get; set; }  
        public int DiscountPercentage { get; set; }

        public DateTime ValidFrom { get; set; }

        public DateTime ValidTill { get; set; }

        public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
}
