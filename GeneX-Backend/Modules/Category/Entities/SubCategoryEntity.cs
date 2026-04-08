using GeneX_Backend.Modules.Products.Entities;
using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Category.Entities
{

    public class SubCategoryEntity
    {
        [Key]
        public Guid SubCategoryId { get; set; }
        public required string SubCategoryName { get; set; }
        public Guid CategoryId { get; set; }
        public CategoryEntity CategoryEntity { get; set; }

        public ICollection<SubCategoryAttributeEntity> SubCategoryAttributeEntities { get; set; } = new List<SubCategoryAttributeEntity>();
        public DateTimeOffset? DeletedAt { get; set; }

        public ICollection<ProductEntity> Products { get; set; } = new List<ProductEntity>();
    }
    
}