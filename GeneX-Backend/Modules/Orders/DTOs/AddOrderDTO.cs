using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Orders.DTOs
{
    public class AddOrderDTO
    {
        public required List<OrderItemsDTO> OrderedItems { get; set; }
        public string? CouponCode { get; set; }
        public required PaymentMode PaymentMode { get; set; }
        public required Guid BillingInfoId { get; set; }

    }

    public class OrderItemsDTO
    {
        public required Guid ProductId { get; set; }
        public required int quantity { get; set; }
    }
}