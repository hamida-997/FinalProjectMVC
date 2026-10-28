using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IApiService _apiService;
        private readonly IAuthService _authService;

        public HomeController(
            IProductService productService,
            ICategoryService categoryService,
            IApiService apiService,
            IAuthService authService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _apiService = apiService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            // Check if user is authenticated and has admin role
            if (!_authService.IsAuthenticated())
            {
                return RedirectToAction("Login", "Auth", new { area = "" });
            }

            var currentUser = _authService.GetCurrentUser();
            var hasAdminAccess = currentUser != null &&
                (currentUser.Roles.Contains("SuperAdmin") || currentUser.Roles.Contains("Admin"));

            if (!hasAdminAccess)
            {
                return RedirectToAction("Index", "Home", new { area = "" });
            }

            // Load dashboard stats
            var products = await _productService.GetAllProductsAsync();
            var categories = await _categoryService.GetAllCategoriesAsync();
            var orders = await _apiService.GetAsync<List<object>>("api/orders");

            var model = new AdminDashboardVM
            {
                TotalProducts = products.Count,
                TotalCategories = categories.Count,
                TotalOrders = orders?.Count ?? 0,
                ActiveProducts = products.Count(p => p.IsAvailable),
                OutOfStockProducts = products.Count(p => p.StockQuantity == 0)
            };

            return View(model);
        }
    }
}
