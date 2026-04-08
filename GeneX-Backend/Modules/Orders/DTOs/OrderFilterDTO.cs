using GeneX_Backend.Modules.Users.Entities;
using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.Orders.DTOs
{
    public class OrderFilterDTO
    {
        public int PageNumber { get; set; } = 1;
        public int PageSize { get; set; } = 1;
        public OrderStatus? Status { get; set; }
        public PaymentMode? PaymentMethod { get; set; }
        public Guid? UserId{ get; set; }
        public Guid? ProductId{ get; set; }
        public DateTime? FromDate { get; set; }
        public DateTime? ToDate { get; set; }
    }
}