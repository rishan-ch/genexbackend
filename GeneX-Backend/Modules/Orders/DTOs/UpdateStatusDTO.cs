using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Orders.DTOs
{
    public class UpdateStatusDTO
    {
        public required Guid OrderId { get; set; }
        public required OrderStatus Status { get; set; }
    }
    
}