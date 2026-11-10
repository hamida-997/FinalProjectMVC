using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IReviewService _reviewService;

        public ProductsController(IProductService productService, ICategoryService categoryService, IReviewService reviewService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _reviewService = reviewService;
        }

        // Shop grid: 9 products per page
        private const int ShopPageSize = 9;

        public async Task<IActionResult> Index(int? categoryId, string? search, string? sortBy, int page = 1)
        {
            search = search?.Trim();

            // Fetch categories; only active ones are shown to customers
            var categories = await _categoryService.GetAllCategoriesAsync();
            var activeCategories = categories.Where(c => c.IsActive).ToList();
            var activeIds = activeCategories.Select(c => c.Id).ToHashSet();

            // An inactive or unknown category in the URL is ignored instead of listing its products
            if (categoryId.HasValue && !activeIds.Contains(categoryId.Value))
            {
                categoryId = null;
            }

            var model = new ProductsListVM
            {
                SelectedCategoryId = categoryId,
                SearchTerm = search,
                SortBy = sortBy,
                Categories = activeCategories
            };

            // Fetch products based on category filter
            var products = categoryId.HasValue
                ? await _productService.GetProductsByCategoryAsync(categoryId.Value)
                : await _productService.GetAllProductsAsync();

            // Only available products in active categories
            products = products.Where(p => p.IsAvailable && activeIds.Contains(p.CategoryId)).ToList();

            // Filter by search term (name, description or category)
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    p.CategoryName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Sort
            products = sortBy switch
            {
                "price-asc" => products.OrderBy(p => p.Price).ToList(),
                "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name-desc" => products.OrderByDescending(p => p.Name).ToList(),
                "newest" => products.OrderByDescending(p => p.CreatedAt).ToList(),
                "rating" => products.OrderByDescending(p => p.AverageRating).ThenByDescending(p => p.ReviewCount).ToList(),
                _ => products.OrderBy(p => p.Name).ToList()
            };

            model.TotalCount = products.Count;
            model.Products = products.Paginate(page, ViewData, ShopPageSize);

            return View(model);
        }

        public async Task<IActionResult> Details(int id, int page = 1)
        {
            var product = await _productService.GetProductByIdAsync(id);

            if (product == null)
            {
                return NotFound();
            }

            // Fetch related products from the same category
            var relatedProducts = await _productService.GetProductsByCategoryAsync(product.CategoryId);

            var model = new ProductDetailsVM
            {
                Product = product,
                RelatedProducts = relatedProducts
                    .Where(p => p.Id != product.Id && p.IsAvailable)
                    .Take(4)
                    .ToList(),
                Reviews = await _reviewService.GetProductReviewsAsync(product.Id)
            };

            // Only ask the API about eligibility when someone is logged in
            if (User.Identity?.IsAuthenticated == true)
            {
                model.Eligibility = await _reviewService.GetEligibilityAsync(product.Id);
                if (model.Eligibility?.ExistingReviewId is int myReviewId)
                {
                    model.MyReview = model.Reviews.FirstOrDefault(r => r.Id == myReviewId);
                }
            }

            // MyReview is looked up in the full list above, then the list is cut to one page.
            // Page links jump back to the reviews section.
            model.Reviews = model.Reviews
                .OrderByDescending(r => r.CreatedAt)
                .Paginate(page, ViewData, fragment: "#reviews");

            return View(model);
        }
    }
}
