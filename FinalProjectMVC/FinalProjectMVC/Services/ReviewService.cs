using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class ReviewService : IReviewService
    {
        private readonly IApiService _apiService;

        public ReviewService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ReviewVM>> GetProductReviewsAsync(int productId)
        {
            var reviews = await _apiService.GetAsync<List<ReviewVM>>($"api/reviews/product/{productId}");
            return reviews ?? new List<ReviewVM>();
        }

        public async Task<List<ReviewVM>> GetAllReviewsAsync(int? productId = null)
        {
            var endpoint = productId.HasValue ? $"api/reviews?productId={productId.Value}" : "api/reviews";
            var reviews = await _apiService.GetAsync<List<ReviewVM>>(endpoint);
            return reviews ?? new List<ReviewVM>();
        }

        public async Task<ReviewEligibilityVM?> GetEligibilityAsync(int productId)
        {
            var result = await _apiService.GetWithResultAsync<ReviewEligibilityVM>($"api/reviews/product/{productId}/eligibility");
            return result.Success ? result.Data : null;
        }

        public Task<ApiResult<ReviewVM>> CreateReviewAsync(ReviewFormVM model)
        {
            return _apiService.PostWithResultAsync<ReviewVM>("api/reviews", new
            {
                productId = model.ProductId,
                rating = model.Rating,
                comment = model.Comment
            });
        }

        public Task<ApiResult<ReviewVM>> UpdateReviewAsync(int reviewId, ReviewFormVM model)
        {
            return _apiService.PutWithResultAsync<ReviewVM>($"api/reviews/{reviewId}", new
            {
                rating = model.Rating,
                comment = model.Comment
            });
        }

        public Task<bool> DeleteReviewAsync(int reviewId)
        {
            return _apiService.DeleteAsync($"api/reviews/{reviewId}");
        }
    }
}
