using GeneX_Backend.Modules.BillingInfo.DTO;
using GeneX_Backend.Modules.BillingInfo.Entity;
using GeneX_Backend.Shared.Enums;

namespace GeneX_Backend.Modules.BillingInfo.Interface
{
    public interface IBillingInfoService
    {
        Task AddBillingInfo(AddBillingDTO addBillingDTO, Guid UserId);
        Task<List<ViewBillingDTO>>? ViewAllBillingInfo(Guid UserId);
        Task DeleteBillingInfo(Guid BillingInfoId);
        Task UpdateBillingInfo(AddBillingDTO addBillingDTO, Guid BillingInfoId);
        Task<ViewBillingDTO> ViewBillingInfo(Guid BillingInfoId);
    }
}