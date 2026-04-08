using GeneX_Backend.Modules.Orders.DTOs;
using GeneX_Backend.Modules.Products.DTOs;

namespace GeneX_Backend.Modules.Orders.Interfaces
{
    public interface IOrderService
    {
        Task AddNewOrder(AddOrderDTO addOrderDTO, Guid UserId);
        Task<PagedResult<ViewOrderDTO>> ViewOrders(Guid? UserId, OrderFilterDTO filter);
        Task UpdateOrderStatus(UpdateStatusDTO updateStatusDTO);
        Task AddRemarks(Guid orderId, string remarks);
    }
}