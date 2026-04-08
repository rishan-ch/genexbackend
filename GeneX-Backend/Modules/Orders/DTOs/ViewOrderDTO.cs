using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Orders.DTOs
{
    public class ViewOrderDTO
    {
        public required Guid UserId { get; set; }
        public required Guid OrderId { get; set; }
        public required string FullName{ get; set; }
        public required string Email{ get; set; }
        public required string Phone { get; set; }
        public required string OrderDateTime { get; set; }
        public required decimal AmountBeforeDiscount { get; set; }
        public string? AppliedCouponCode { get; set; }
        public decimal? CouponDiscountPercent { get; set; }
        public required decimal AmountAfterDiscount { get; set; }
        public required OrderStatus Status { get; set; }
        public required PaymentMode PaymentMethod { get; set; }
        public required Guid BillingInfoId{ get; set; }
        public string? Remarks{ get; set; }
        public List<ViewItemDTO> OrderItems { get; set; }

    }

    public class ViewItemDTO
    {
        public required Guid ProductId { get; set; }
        public required string ProductName{ get; set; }
        public string? ProductImageUrl{ get; set; }
        public required int Quantity { get; set; }
        public required decimal UnitPrice{ get; set; }
        public required decimal LineTotal { get; set; }
    }
}