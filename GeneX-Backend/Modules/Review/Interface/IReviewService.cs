using GeneX_Backend.Modules.Review.DTOs;

namespace GeneX_Backend.Modules.Review.Interface
{
    public interface IReviewService
    {
        Task AddReview(AddReviewDTO AddReviewDTO, Guid UserId);
        Task<ViewReviewDTO> ViewReviewByProduct(Guid ProductId);
        Task AddReviewResponse(AddResponseDTO addResponseDTO, Guid UserId, string Role);


    }
}