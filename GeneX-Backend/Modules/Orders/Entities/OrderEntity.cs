using System.ComponentModel.DataAnnotations;
using GeneX_Backend.Modules.BillingInfo.Entity;
using GeneX_Backend.Modules.Coupon.Entities;
using GeneX_Backend.Modules.Products.Entities;
using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Orders.Entities
{
    public class OrderEntity
    {
        [Key]
        public Guid OrderId { get; set; }
        public required DateTime OrderDateTime { get; set; }
        public required Guid UserId { get; set; }
        public required decimal AmountBeforeDiscount { get; set; }
        public required decimal AmountAfterDiscount { get; set; }
        public required OrderStatus Status { get; set; }
        public required PaymentMode PaymentMethod { get; set; }
        public Guid? CouponId { get; set; }
        public required Guid BillingInfoId{ get; set; }
        public string? Remarks{ get; set; }
        public ICollection<OrderItemEntity> OrderItems { get; set; } = new List<OrderItemEntity>();



        public UserEntity User { get; set; }
        public CouponEntity? Coupon{ get; set; }
        public ICollection<ProductEntity>? ProductEntity { get; set; } = new List<ProductEntity>();
        public BillingInfoEntity BillingInfo{ get; set; }
    }

    public class OrderItemEntity
    {
        [Key]
        public Guid OrderItemId { get; set; }
        public required Guid OrderId { get; set; }
        public required Guid ProductId { get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice{ get; set; }
        public required decimal LineTotal { get; set; }

        public OrderEntity Order{ get; set; }
        public ProductEntity Product { get; set; }
    }
}