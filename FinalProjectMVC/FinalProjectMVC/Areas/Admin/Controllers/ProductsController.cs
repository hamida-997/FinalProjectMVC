using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    public class ProductsController : Controller
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly IAuthService _authService;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IAuthService authService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _authService = authService;
        }

        public async Task<IActionResult> Index()
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            await LoadCategoriesDropdown();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductVM model)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                await LoadCategoriesDropdown();
                return View(model);
            }

            var result = await _productService.CreateProductAsync(model);

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to create product. Please try again.");
                await LoadCategoriesDropdown();
                return View(model);
            }

            TempData["SuccessMessage"] = $"Product '{result.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task LoadCategoriesDropdown()
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var activeCategories = categories.Where(c => c.IsActive).ToList();

            ViewBag.Categories = new SelectList(activeCategories, "Id", "Name");
        }

        private bool HasAdminAccess()
        {
            if (!_authService.IsAuthenticated())
            {
                return false;
            }

            var currentUser = _authService.GetCurrentUser();
            return currentUser != null &&
                (currentUser.Roles.Contains("SuperAdmin") || currentUser.Roles.Contains("Admin"));
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Auth", new { area = "" });
        }
    }
}
