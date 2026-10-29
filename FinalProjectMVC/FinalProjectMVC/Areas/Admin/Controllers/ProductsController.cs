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
        private readonly IProductImageService _productImageService;
        private readonly IAuthService _authService;

        public ProductsController(
            IProductService productService,
            ICategoryService categoryService,
            IProductImageService productImageService,
            IAuthService authService)
        {
            _productService = productService;
            _categoryService = categoryService;
            _productImageService = productImageService;
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

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = $"Product with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateProductVM
            {
                Id = product.Id,
                Name = product.Name,
                Description = product.Description,
                Price = product.Price,
                StockQuantity = product.StockQuantity,
                IsAvailable = product.IsAvailable,
                CategoryId = product.CategoryId
            };

            await LoadCategoriesDropdown();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateProductVM model)
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

            var result = await _productService.UpdateProductAsync(model.Id, model);

            if (result == null)
            {
                ModelState.AddModelError(string.Empty, "Failed to update product. Please try again.");
                await LoadCategoriesDropdown();
                return View(model);
            }

            TempData["SuccessMessage"] = $"Product '{result.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var result = await _productService.DeleteProductAsync(id);

            if (result)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete product. It may have associated orders.";
            }

            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Images(int id)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                TempData["ErrorMessage"] = $"Product with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            var images = await _productImageService.GetImagesByProductAsync(id);

            var model = new ProductImagesVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Images = images
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(int productId, IFormFile file, bool isPrimary = false)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            if (file == null || file.Length == 0)
            {
                TempData["ErrorMessage"] = "Please select a file to upload.";
                return RedirectToAction(nameof(Images), new { id = productId });
            }

            var result = await _productImageService.UploadImageAsync(productId, file, isPrimary);

            if (result != null)
            {
                TempData["SuccessMessage"] = "Image uploaded successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to upload image. Check file size and format.";
            }

            return RedirectToAction(nameof(Images), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SetPrimaryImage(int productId, int imageId)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var result = await _productImageService.SetPrimaryAsync(productId, imageId);

            if (result)
            {
                TempData["SuccessMessage"] = "Primary image updated.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to update primary image.";
            }

            return RedirectToAction(nameof(Images), new { id = productId });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteImage(int imageId, int productId)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var result = await _productImageService.DeleteImageAsync(imageId);

            if (result)
            {
                TempData["SuccessMessage"] = "Image deleted successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete image.";
            }

            return RedirectToAction(nameof(Images), new { id = productId });
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
