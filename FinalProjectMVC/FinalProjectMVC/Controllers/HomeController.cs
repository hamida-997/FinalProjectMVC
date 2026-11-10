using Microsoft.AspNetCore.Mvc;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using System.Diagnostics;

namespace FinalProjectMVC.Controllers
{
    public class HomeController : Controller
    {
        private const int FeaturedCount = 6;

        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;

        public HomeController(IProductService productService, ICategoryService categoryService, IReviewService reviewService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        public async Task<IActionResult> Index()
        {
            var products = await _productService.GetAllProductsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var activeIds = categories.Where(c => c.IsActive).Select(c => c.Id).ToHashSet();

            // Same rule as the shop: only available products in active categories
            var buyable = products
                .Where(p => p.IsAvailable && activeIds.Contains(p.CategoryId))
                .ToList();

            var model = new HomeVM
            {
                ProductCount = buyable.Count,
                // In-stock first, then best rated, then newest
                FeaturedProducts = buyable
                    .OrderByDescending(p => p.StockQuantity > 0)
                    .ThenByDescending(p => p.AverageRating)
                    .ThenByDescending(p => p.ReviewCount)
                    .ThenByDescending(p => p.CreatedAt)
                    .Take(FeaturedCount)
                    .ToList(),
                Testimonials = await _reviewService.GetFeaturedReviewsAsync(3)
            };

            return View(model);
        }

        public async Task<IActionResult> About()
        {
            var products = await _productService.GetAllProductsAsync();
            var categories = (await _categoryService.GetAllCategoriesAsync()).Where(c => c.IsActive).ToList();
            var activeIds = categories.Select(c => c.Id).ToHashSet();
            var buyable = products.Where(p => p.IsAvailable && activeIds.Contains(p.CategoryId)).ToList();

            // Totals come from each product's own rating, so no extra API call is needed
            var reviewCount = products.Sum(p => p.ReviewCount);
            var model = new AboutVM
            {
                ProductCount = buyable.Count,
                CategoryCount = categories.Count,
                ReviewCount = reviewCount,
                AverageRating = reviewCount == 0 ? 0 : products.Sum(p => p.AverageRating * p.ReviewCount) / reviewCount,
                TopProducts = buyable
                    .Where(p => p.ReviewCount > 0)
                    .OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount)
                    .Take(3).ToList(),
                Categories = categories.OrderBy(c => c.Name).ToList(),
                Reviews = await _reviewService.GetFeaturedReviewsAsync(3)
            };

            // No reviewed products yet: show the newest ones instead
            if (model.TopProducts.Count == 0)
            {
                model.TopProducts = buyable.OrderByDescending(p => p.CreatedAt).Take(3).ToList();
            }

            return View(model);
        }

        public IActionResult AccessDenied()
        {
            Response.StatusCode = StatusCodes.Status403Forbidden;
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorVM { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
