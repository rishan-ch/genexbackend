using GeneX_Backend.Modules.Category.Entities;
using GeneX_Backend.Modules.Discount.Entities;
using GeneX_Backend.Modules.Orders.Entities;
using System.ComponentModel.DataAnnotations;

namespace GeneX_Backend.Modules.Products.Entities
{
    public class ProductEntity
    {
        [Key]
        public Guid ProductId { get; set; }

        public Guid SubCategoryId { get; set; }

        public string ProductName { get; set; }

        public string ProductDescription { get; set; }

        public int ProductQuantity { get; set; }

        public Guid? DiscountId { get; set; }

        // set as default false intially.
        public bool IsDeleted { get; set; } = false;

        public DateTime? DeletedAt { get; set; }

        public decimal ProductUnitPrice { get; set; }
        public required bool HotDeals{ get; set; }
        public required DateTime AddedDate{ get; set; }

        public SubCategoryEntity SubCategoryEntity { get; set; }

        public DiscountEntity? DiscountEntity { get; set; }

        public ICollection<OrderEntity> OrderEntity { get; set; } = new List<OrderEntity>();
        public ICollection<ReviewEntity> ReviewEntities { get; set; } = new List<ReviewEntity>();

        public ICollection<ProductAttributeEntity> ProductAttributes { get; set; } = new List<ProductAttributeEntity>();
        public ICollection<ProductImagesEntity> ProductImages { get; set; } = new List<ProductImagesEntity>();
       



    }
}
