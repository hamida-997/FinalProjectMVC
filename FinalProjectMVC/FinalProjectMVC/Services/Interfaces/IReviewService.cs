using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IReviewService
    {
        Task<List<ReviewVM>> GetProductReviewsAsync(int productId);

        // Null when the user isn't logged in or the check failed
        Task<ReviewEligibilityVM?> GetEligibilityAsync(int productId);

        Task<ApiResult<ReviewVM>> CreateReviewAsync(ReviewFormVM model);
        Task<ApiResult<ReviewVM>> UpdateReviewAsync(int reviewId, ReviewFormVM model);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
