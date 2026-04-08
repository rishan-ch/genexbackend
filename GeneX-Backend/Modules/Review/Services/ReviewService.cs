using System.Security.Principal;
using GeneX_Backend.Infrastructure.Data;
using GeneX_Backend.Modules.Orders.Entities;
using GeneX_Backend.Modules.Review.DTOs;
using GeneX_Backend.Shared.Exceptions;
using Microsoft.EntityFrameworkCore;

namespace GeneX_Backend.Modules.Review.Interface
{
    public class ReviewService : IReviewService
    {

        private readonly AppDbContext _appDbContext;

        public ReviewService(AppDbContext appDbContext)
        {
            _appDbContext = appDbContext;
        }

        public async Task AddReview(AddReviewDTO addReviewDTO, Guid UserId)
        {


            bool validReview = CanReview(UserId, addReviewDTO.ProductId);

            if (!validReview) throw new UnauthorizedAccessException("Product purchase must be completed before reviewing");

            _appDbContext.Reviews.Add(
                new ReviewEntity()
                {
                    Description = addReviewDTO.Description,
                    ProductId = addReviewDTO.ProductId,
                    ReviewDate = DateTime.UtcNow,
                    StarCount = addReviewDTO.StarCount,
                    UserId = UserId
                }
            );
            await _appDbContext.SaveChangesAsync();
        }

        public bool CanReview(Guid userId, Guid productId)
        {
            return _appDbContext.OrderItems
                .Any(oi => oi.ProductId == productId &&
                           oi.Order.UserId == userId &&
                           oi.Order.Status == Shared.Enums.OrderStatus.Completed);
        }


        public async Task<ViewReviewDTO> ViewReviewByProduct(Guid ProductId)
        {
            var reviews = await _appDbContext.Reviews
                .Where(rev => rev.ProductId == ProductId)
                .Include(r => r.User)
                .ToListAsync();

            var productReviews = new List<ReviewList>();

            foreach (var r in reviews)
            {
                var reviewList = new ReviewList()
                {
                    ReviewId = r.ReviewId,
                    Description = r.Description,
                    ReviewDate = r.ReviewDate,
                    StarCount = r.StarCount,
                    Username = r.User.FirstName + " " + r.User.LastName,
                    Responses = await GetResponsesByReview(r.ReviewId)
                };

                productReviews.Add(reviewList);
            }

            decimal avg = CalculateAvgStar(productReviews);

            return new ViewReviewDTO()
            {
                Average = avg,
                ReviewList = productReviews
            };
        }


        public async Task<List<ResponseList>?> GetResponsesByReview(Guid ReviewId)
        {
            var responses = await _appDbContext.ReviewResponses
                .Where(r => r.ReviewId == ReviewId)
                .Select(re => new
                {
                    re.Description,
                    re.ResponseDate,
                    UserName = re.User.FirstName + " " + re.User.LastName,
                    Roles = _appDbContext.UserRoles
                                .Where(ur => ur.UserId == re.User.Id)
                                .Join(_appDbContext.Roles,
                                      ur => ur.RoleId,
                                      role => role.Id,
                                      (ur, role) => role.Name)
                                .ToList()
                })
                .ToListAsync();

            var responseList = responses.Select(r => new ResponseList
            {
                Username = r.UserName,
                ResponseDate = r.ResponseDate,
                Description = r.Description,
                isAdmin = r.Roles.Any(role => role == "SuperAdmin" || role == "Admin")
                          && !r.Roles.Any(role => role.Contains("Customer"))
            }).ToList();

            return responseList.Any() ? responseList : null;
        }

        public decimal CalculateAvgStar(List<ReviewList> reviews)
        {
            if (reviews == null || reviews.Count == 0)
                return 0;

            int total = 0;
            foreach (var review in reviews)
            {
                total += review.StarCount;
            }

            decimal average = (decimal)total / reviews.Count;
            return Math.Round(average, 1);
        }


        //ads the replies to a review can be from admin or user
        public async Task AddReviewResponse(AddResponseDTO addResponseDTO, Guid UserId, string Role)
        {
            using var transaction = await _appDbContext.Database.BeginTransactionAsync();
            try
            {

                await VerifyReview(addResponseDTO.ReviewId);

                _appDbContext.ReviewResponses.Add(
                    new ReviewResponseEntity
                    {
                        UserId = UserId,
                        Role = Role,
                        ResponseDate = DateTime.UtcNow,
                        Description = addResponseDTO.Description,
                        ReviewId = addResponseDTO.ReviewId
                    }
                );

                await _appDbContext.SaveChangesAsync();

                await transaction.CommitAsync();

            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }

        }

        //verifies that the review is existing in the system
        public async Task VerifyReview(Guid ReviewId)
        {
            ReviewEntity? review = await _appDbContext.Reviews.FirstOrDefaultAsync(
                r => r.ReviewId == ReviewId
            );
            if (review == null) throw new NotFoundException("The required review doesn't exists");
        }
    }
}