using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Category.Entities
{
    public class CategoryEntity
    {
        [Key]
        public Guid CategoryId { get; set; }
        public required string CategoryName { get; set; }
        public ICollection<SubCategoryEntity> SubCategories { get; set; } = new List<SubCategoryEntity>();
        public DateTimeOffset? DeletedAt { get; set; }
    }
}