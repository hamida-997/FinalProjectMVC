using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
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

        // GET: Admin/Products?search=cake&stock=out
        public async Task<IActionResult> Index(string? search, string? stock, int page = 1)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var all = await _productService.GetAllProductsAsync();
            IEnumerable<ProductVM> products = all;

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                products = products.Where(p =>
                    p.Name.Contains(term, StringComparison.OrdinalIgnoreCase) ||
                    p.CategoryName.Contains(term, StringComparison.OrdinalIgnoreCase));
            }

            products = stock switch
            {
                "out" => products.Where(p => p.StockQuantity == 0),
                "low" => products.Where(p => p.StockQuantity > 0 && p.StockQuantity < 10),
                "available" => products.Where(p => p.IsAvailable),
                "hidden" => products.Where(p => !p.IsAvailable),
                _ => products
            };

            // Stat cards always describe the whole catalog, not the filtered list
            ViewBag.AllProducts = all;
            ViewBag.Search = search;
            ViewBag.Stock = stock;
            return View(products.Paginate(page, ViewData));
        }

        [HttpGet]
        public async Task<IActionResult> Create()
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            await LoadCategoriesDropdown();
            // Pass a model so defaults apply (e.g. "Available for sale" starts checked)
            return View(new CreateProductVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateProductVM model)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            ValidateImages(model.Images);

            if (!ModelState.IsValid)
            {
                await LoadCategoriesDropdown();
                return View(model);
            }

            var result = await _productService.CreateProductAsync(model);

            if (result.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login", "Auth", new { area = "", returnUrl = Url.Action("Create", "Products", new { area = "Admin" }) });
            }

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create product. Please try again.");
                await LoadCategoriesDropdown();
                return View(model);
            }

            var product = result.Data;

            // The photo the admin picked becomes the main (cover) photo
            var primaryIndex = model.PrimaryImageIndex >= 0 && model.PrimaryImageIndex < model.Images.Count
                ? model.PrimaryImageIndex
                : 0;

            var uploaded = 0;
            var primaryUploaded = false;
            ProductImageVM? firstUploaded = null;
            for (var i = 0; i < model.Images.Count; i++)
            {
                var isPrimary = i == primaryIndex;
                var image = await _productImageService.UploadImageAsync(product.Id, model.Images[i], isPrimary);
                if (image != null)
                {
                    uploaded++;
                    firstUploaded ??= image;
                    if (isPrimary) primaryUploaded = true;
                }
            }

            // If the chosen main photo failed, use another one so the product still has a cover
            if (!primaryUploaded && firstUploaded != null)
            {
                await _productImageService.SetPrimaryAsync(product.Id, firstUploaded.Id);
            }

            if (uploaded == 0)
            {
                // A product must have a photo: undo the create so no photo-less product is left behind
                await _productService.DeleteProductAsync(product.Id);
                ModelState.AddModelError(string.Empty, "The photos could not be uploaded, so the product was not created. Please try again.");
                await LoadCategoriesDropdown();
                return View(model);
            }

            TempData["SuccessMessage"] = uploaded == model.Images.Count
                ? $"Product '{product.Name}' created with {uploaded} photo{(uploaded != 1 ? "s" : "")}."
                : $"Product '{product.Name}' created, but only {uploaded} of {model.Images.Count} photos uploaded.";
            return RedirectToAction(nameof(Index));
        }

        private void ValidateImages(List<IFormFile> images)
        {
            var key = nameof(CreateProductVM.Images);
            images.RemoveAll(f => f == null || f.Length == 0);

            if (images.Count == 0)
            {
                ModelState.AddModelError(key, "Please add at least one product photo.");
                return;
            }

            if (images.Count > CreateProductVM.MaxImages)
            {
                ModelState.AddModelError(key, $"You can upload up to {CreateProductVM.MaxImages} photos.");
            }

            foreach (var file in images)
            {
                var ext = Path.GetExtension(file.FileName).ToLowerInvariant();
                if (!CreateProductVM.AllowedImageExtensions.Contains(ext))
                {
                    ModelState.AddModelError(key, $"'{file.FileName}' is not a supported image (JPG, PNG, GIF, WEBP).");
                }
                else if (file.Length > CreateProductVM.MaxImageBytes)
                {
                    ModelState.AddModelError(key, $"'{file.FileName}' is larger than 5 MB.");
                }
            }
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

            ViewBag.Images = product.Images;
            await LoadCategoriesDropdown(product.CategoryId);
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
                return await RedisplayEditAsync(model);
            }

            var result = await _productService.UpdateProductAsync(model.Id, model);

            if (result.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
            {
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                return RedirectToAction("Login", "Auth", new { area = "", returnUrl = Url.Action("Edit", "Products", new { area = "Admin", id = model.Id }) });
            }

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update product. Please try again.");
                return await RedisplayEditAsync(model);
            }

            TempData["SuccessMessage"] = $"Product '{result.Data.Name}' updated successfully.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<IActionResult> RedisplayEditAsync(UpdateProductVM model)
        {
            var product = await _productService.GetProductByIdAsync(model.Id);
            ViewBag.Images = product?.Images ?? new List<ProductImageVM>();
            await LoadCategoriesDropdown(model.CategoryId);
            return View(nameof(Edit), model);
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

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Product deleted successfully.";
                return RedirectToAction(nameof(Index));
            }

            // e.g. "'Cake' appears in past orders, so it can't be deleted..."
            TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete product.";
            return result.StatusCode == StatusCodes.Status404NotFound
                ? RedirectToAction(nameof(Index))
                : RedirectToAction(nameof(Edit), new { id });
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

            var existing = await _productImageService.GetImagesByProductAsync(productId);
            if (existing.Count >= CreateProductVM.MaxImages)
            {
                TempData["ErrorMessage"] = $"A product can have up to {CreateProductVM.MaxImages} photos. Delete one first.";
                return RedirectToAction(nameof(Images), new { id = productId });
            }

            // The first photo of a product always becomes the main one
            if (existing.Count == 0) isPrimary = true;

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

        // keepCategoryId: when editing, keep the product's current category in the list even if it
        // is inactive, otherwise the dropdown shows nothing selected and saving fails validation
        private async Task LoadCategoriesDropdown(int? keepCategoryId = null)
        {
            var categories = await _categoryService.GetAllCategoriesAsync();
            var activeCategories = categories.Where(c => c.IsActive || c.Id == keepCategoryId).ToList();

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
