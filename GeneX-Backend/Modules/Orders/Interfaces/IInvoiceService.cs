using GeneX_Backend.Modules.Orders.DTOs;

namespace GeneX_Backend.Modules.Orders.Interfaces
{
    public interface IInvoiceService
    {
        Task<string> GenerateInvoiceAsync(Guid orderId);
        Task<byte[]> GenerateInvoicePdfAsync(Guid orderId);
    }
}