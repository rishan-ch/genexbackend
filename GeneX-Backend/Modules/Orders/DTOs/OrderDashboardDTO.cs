using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Orders.DTOs
{
    public class OrderDashboardDTO
    {
        public required Guid OrderId { get; set; }
        public required string FullName { get; set; }
        public required string OrderDateTime { get; set; }
        public required decimal AmountAfterDiscount { get; set; }
        public required OrderStatus Status { get; set; }

    }
}