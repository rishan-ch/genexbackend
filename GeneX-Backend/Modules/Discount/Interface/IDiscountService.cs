using GeneX_Backend.Modules.Discount.DTOs;
using GeneX_Backend.Modules.Products.DTOs;

namespace GeneX_Backend.Modules.Discount.Interface
{
    public interface IDiscountService
    {
        Task<bool> AddDiscountAsync(AddDiscountDTO discount);
        Task<PagedResult<ViewDiscountDTO>> GetAllDiscount(ViewDiscountFilterDto filter);
        Task<ViewDiscountDTO> GetDiscountById(Guid DiscountId);

        Task CleanUpExistingDiscountAsync();
        Task<bool> DeleteDiscountAsync(Guid discountId);

        //Task<bool> UpdateDiscountAsync(Guid discountId, UpdateDiscountDTO discountDto);

    }
}
