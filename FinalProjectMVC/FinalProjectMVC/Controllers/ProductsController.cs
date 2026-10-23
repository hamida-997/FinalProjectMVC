using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;

        public ProductsController(IProductService productService, ICategoryService categoryService)
        {
            _productService = productService;
            _categoryService = categoryService;
        }

        public async Task<IActionResult> Index(int? categoryId, string? search, string? sortBy)
        {
            var model = new ProductsListVM
            {
                SelectedCategoryId = categoryId,
                SearchTerm = search,
                SortBy = sortBy
            };

            // Fetch categories
            var categories = await _categoryService.GetAllCategoriesAsync();
            model.Categories = categories.Where(c => c.IsActive).ToList();

            // Fetch products based on category filter
            var products = categoryId.HasValue && categoryId.Value > 0
                ? await _productService.GetProductsByCategoryAsync(categoryId.Value)
                : await _productService.GetAllProductsAsync();

            // Filter by search term
            if (!string.IsNullOrWhiteSpace(search))
            {
                products = products.Where(p =>
                    p.Name.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                    (p.Description != null && p.Description.Contains(search, StringComparison.OrdinalIgnoreCase))
                ).ToList();
            }

            // Only show available products
            products = products.Where(p => p.IsAvailable).ToList();

            // Sort
            products = sortBy switch
            {
                "price-asc" => products.OrderBy(p => p.Price).ToList(),
                "price-desc" => products.OrderByDescending(p => p.Price).ToList(),
                "name-desc" => products.OrderByDescending(p => p.Name).ToList(),
                "newest" => products.OrderByDescending(p => p.CreatedAt).ToList(),
                _ => products.OrderBy(p => p.Name).ToList()
            };

            model.Products = products;

            return View(model);
        }

        public async Task<IActionResult> Details(int id)
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
                    .ToList()
            };

            return View(model);
        }
    }
}
