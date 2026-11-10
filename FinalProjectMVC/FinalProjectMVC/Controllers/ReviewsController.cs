using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    // Handles the review form on the product details page.
    // The API enforces the buyer check and ownership; this controller only relays.
    [Authorize]
    public class ReviewsController : Controller
    {
        private const string ReviewErrorKey = "ReviewError";
        private readonly IReviewService _reviewService;
        private readonly IAuthService _authService;

        public ReviewsController(IReviewService reviewService, IAuthService authService)
        {
            _reviewService = reviewService;
            _authService = authService;
        }

        // POST: /Reviews/Save  (create or update)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Save(ReviewFormVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData[ReviewErrorKey] = string.Join(" ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return BackToReviews(model.ProductId);
            }

            var result = model.ReviewId.HasValue
                ? await _reviewService.UpdateReviewAsync(model.ReviewId.Value, model)
                : await _reviewService.CreateReviewAsync(model);

            if (result.StatusCode == StatusCodes.Status401Unauthorized)
            {
                await _authService.LogoutAsync();
                return RedirectToAction("Login", "Auth", new { returnUrl = DetailsUrl(model.ProductId) });
            }

            if (!result.Success)
            {
                // e.g. "You can only review products you have bought."
                TempData[ReviewErrorKey] = result.ErrorMessage ?? "Your review could not be saved.";
                return BackToReviews(model.ProductId);
            }

            TempData["SuccessMessage"] = model.ReviewId.HasValue
                ? "Your review was updated."
                : "Thanks! Your review was posted.";
            return BackToReviews(model.ProductId);
        }

        // POST: /Reviews/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int reviewId, int productId)
        {
            if (await _reviewService.DeleteReviewAsync(reviewId))
            {
                TempData["SuccessMessage"] = "Your review was deleted.";
            }
            else
            {
                TempData[ReviewErrorKey] = "Your review could not be deleted.";
            }

            return BackToReviews(productId);
        }

        private string DetailsUrl(int productId) =>
            Url.Action("Details", "Products", new { id = productId }) ?? "/";

        private IActionResult BackToReviews(int productId) =>
            Redirect(DetailsUrl(productId) + "#reviews");
    }
}
