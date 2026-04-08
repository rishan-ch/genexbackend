using GeneX_Backend.Modules.Products.Entities;
using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Category.Entities
{

    public class SubCategoryAttributeEntity
    {
        [Key]
        public Guid AttributeId { get; set; }
        public required string AttributeName { get; set; }

        //to be stored in a json 
        public string? PossibleValuesJson { get; set; }
        public required string Type { get; set; }
        public required bool IsRequired { get; set; }
        public required Guid SubCategoryId { get; set; }
        public SubCategoryEntity SubCategoryEntity { get; set; }

        public ICollection<ProductAttributeEntity> ProductAttributes { get; set; } = new List<ProductAttributeEntity>();

    }

}