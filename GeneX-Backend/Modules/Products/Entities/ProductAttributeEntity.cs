using GeneX_Backend.Modules.Category.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace GeneX_Backend.Modules.Products.Entities
{
    public class ProductAttributeEntity
    {
        [Key]
        public Guid ProductAttributeId { get; set; }

        public required Guid ProductId { get; set; }

        public Guid SubCategoryAttributeId { get; set; }

        public string SubCategoryAttributeName { get; set; }

        public string ProductAttributeType { get; set; }

        public required string? ProductAttributeValue { get; set; }


        public ProductEntity ProductEntity { get; set; }

        public SubCategoryAttributeEntity SubCategoryAttribute { get; set; }

    }
}
