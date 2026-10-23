using FinalProjectMVC.ViewModels;
using FinalProjectMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class ProductsController : Controller
    {
        private readonly IApiService _apiService;

        public ProductsController(IApiService apiService)
        {
            _apiService = apiService;
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
            var categories = await _apiService.GetAsync<List<CategoryVM>>("api/categories");
            model.Categories = categories?.Where(c => c.IsActive).ToList() ?? new List<CategoryVM>();

            // Fetch products based on filters
            List<ProductVM>? products;
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                products = await _apiService.GetAsync<List<ProductVM>>($"api/products/Category/{categoryId.Value}");
            }
            else
            {
                products = await _apiService.GetAsync<List<ProductVM>>("api/products");
            }

            products ??= new List<ProductVM>();

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
            var product = await _apiService.GetAsync<ProductVM>($"api/products/{id}");

            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }
    }
}
