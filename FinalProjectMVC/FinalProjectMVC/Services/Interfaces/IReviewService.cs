using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewVM>> GetProductReviewsAsync(int productId);

        // Public: recent 4-5 star reviews with a comment, for the home page
        Task<List<ReviewVM>> GetFeaturedReviewsAsync(int count = 3);

        // Admin only: all reviews, optionally for one product
        Task<List<ReviewVM>> GetAllReviewsAsync(int? productId = null);

        // Null when the user isn't logged in or the check failed
        Task<ReviewEligibilityVM?> GetEligibilityAsync(int productId);

        Task<ApiResult<ReviewVM>> CreateReviewAsync(ReviewFormVM model);
        Task<ApiResult<ReviewVM>> UpdateReviewAsync(int reviewId, ReviewFormVM model);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
