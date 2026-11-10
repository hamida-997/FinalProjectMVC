using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class CategoriesController : Controller
    {
        private readonly ICategoryService _categoryService;
        private readonly IAuthService _authService;

        public CategoriesController(ICategoryService categoryService, IAuthService authService)
        {
            _categoryService = categoryService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(int page = 1)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var categories = await _categoryService.GetAllCategoriesAsync();
            return View(categories.Paginate(page, ViewData));
        }

        [HttpGet]
        public IActionResult Create()
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            // Pass a model so "Mark as active" starts checked (CreateCategoryVM.IsActive = true)
            return View(new CreateCategoryVM());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateCategoryVM model)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.CreateCategoryAsync(model);

            if (result.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
            {
                return await SessionExpiredAsync();
            }

            if (!result.Success || result.Data == null)
            {
                // Shows the API's reason, e.g. "A category named 'Cakes' already exists."
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to create category. Please try again.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Category '{result.Data.Name}' created successfully.";
            return RedirectToAction(nameof(Index));
        }

        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var category = await _categoryService.GetCategoryByIdAsync(id);
            if (category == null)
            {
                TempData["ErrorMessage"] = $"Category with ID {id} not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new UpdateCategoryVM
            {
                Id = category.Id,
                Name = category.Name,
                Description = category.Description,
                IsActive = category.IsActive
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(UpdateCategoryVM model)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var result = await _categoryService.UpdateCategoryAsync(model.Id, model);

            if (result.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
            {
                return await SessionExpiredAsync();
            }

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Failed to update category. Please try again.");
                return View(model);
            }

            TempData["SuccessMessage"] = $"Category '{result.Data.Name}' updated successfully.";
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

            var result = await _categoryService.DeleteCategoryAsync(id);

            if (result.StatusCode is StatusCodes.Status401Unauthorized or StatusCodes.Status403Forbidden)
            {
                return await SessionExpiredAsync();
            }

            if (result.Success)
            {
                TempData["SuccessMessage"] = "Category deleted successfully.";
            }
            else
            {
                // e.g. "'Cakes' still has 3 products. Move or delete them first..."
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to delete category.";
            }

            return RedirectToAction(nameof(Index));
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

        // The API token (60 min) can expire while the MVC cookie is still valid
        private async Task<IActionResult> SessionExpiredAsync()
        {
            await _authService.LogoutAsync();
            TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
            return RedirectToAction("Login", "Auth", new { area = "", returnUrl = Url.Action("Index", "Categories", new { area = "Admin" }) });
        }
    }
}
