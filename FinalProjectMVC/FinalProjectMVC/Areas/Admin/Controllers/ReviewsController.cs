using FinalProjectMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class ReviewsController : Controller
    {
        private readonly IReviewService _reviewService;
        private readonly IProductService _productService;

        public ReviewsController(IReviewService reviewService, IProductService productService)
        {
            _reviewService = reviewService;
            _productService = productService;
        }

        // GET: Admin/Reviews?productId=5&rating=1
        public async Task<IActionResult> Index(int? productId, int? rating)
        {
            var reviews = await _reviewService.GetAllReviewsAsync(productId);

            if (rating is >= 1 and <= 5)
            {
                reviews = reviews.Where(r => r.Rating == rating.Value).ToList();
            }

            var products = await _productService.GetAllProductsAsync();
            ViewBag.Products = new SelectList(products.OrderBy(p => p.Name), "Id", "Name", productId);
            ViewBag.ProductId = productId;
            ViewBag.Rating = rating;

            return View(reviews);
        }

        // POST: Admin/Reviews/Delete
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id, int? productId, int? rating)
        {
            if (await _reviewService.DeleteReviewAsync(id))
            {
                TempData["SuccessMessage"] = "Review deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "The review could not be deleted. It may already be gone.";
            }

            // Keep the admin's current filters
            return RedirectToAction(nameof(Index), new { productId, rating });
        }
    }
}
