using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class DiscountsController : Controller
    {
        private readonly IDiscountService _discountService;

        public DiscountsController(IDiscountService discountService)
        {
            _discountService = discountService;
        }

        // GET: Admin/Discounts
        public async Task<IActionResult> Index()
        {
            var codes = await _discountService.GetAllAsync();
            return View(codes);
        }

        // GET: Admin/Discounts/Create
        [HttpGet]
        public IActionResult Create()
        {
            return View("Form", new SaveDiscountCodeVM());
        }

        // POST: Admin/Discounts/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(SaveDiscountCodeVM model)
        {
            ValidateForm(model);
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var result = await _discountService.CreateAsync(model);
            if (!result.Success)
            {
                // e.g. "The code 'SWEET10' already exists."
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The discount code could not be created.");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = $"Discount code '{result.Data?.Code}' created.";
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Discounts/Edit/5
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var code = await _discountService.GetByIdAsync(id);
            if (code == null)
            {
                TempData["ErrorMessage"] = "Discount code not found.";
                return RedirectToAction(nameof(Index));
            }

            var model = new SaveDiscountCodeVM
            {
                Id = code.Id,
                Code = code.Code,
                Description = code.Description,
                Type = code.Type,
                Value = code.Value,
                MinOrderAmount = code.MinOrderAmount,
                MaxUses = code.MaxUses,
                // The API returns UTC; the form shows local time
                ExpiresAt = code.ExpiresAt.HasValue
                    ? DateTime.SpecifyKind(code.ExpiresAt.Value, DateTimeKind.Utc).ToLocalTime()
                    : null,
                IsActive = code.IsActive,
                UsedCount = code.UsedCount
            };

            return View("Form", model);
        }

        // POST: Admin/Discounts/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, SaveDiscountCodeVM model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            ValidateForm(model);
            if (!ModelState.IsValid)
            {
                return View("Form", model);
            }

            var result = await _discountService.UpdateAsync(id, model);
            if (!result.Success)
            {
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "The discount code could not be updated.");
                return View("Form", model);
            }

            TempData["SuccessMessage"] = $"Discount code '{result.Data?.Code}' updated.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Discounts/ToggleActive/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleActive(int id)
        {
            var result = await _discountService.ToggleActiveAsync(id);
            if (result.Success && result.Data != null)
            {
                TempData["SuccessMessage"] = $"'{result.Data.Code}' is now {(result.Data.IsActive ? "active" : "inactive")}.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "The status could not be changed.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Discounts/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            if (await _discountService.DeleteAsync(id))
            {
                TempData["SuccessMessage"] = "Discount code deleted. Past orders keep their discount.";
            }
            else
            {
                TempData["ErrorMessage"] = "The discount code could not be deleted.";
            }

            return RedirectToAction(nameof(Index));
        }

        // Rules the attributes can't express; the API checks them again
        private void ValidateForm(SaveDiscountCodeVM model)
        {
            if (model.Type == DiscountTypeVM.Percentage && model.Value > 100)
            {
                ModelState.AddModelError(nameof(model.Value), "A percentage discount cannot be more than 100%.");
            }

            if (model.ExpiresAt.HasValue && model.ExpiresAt.Value <= DateTime.Now && model.Id == 0)
            {
                ModelState.AddModelError(nameof(model.ExpiresAt), "The expiry date must be in the future.");
            }
        }
    }
}
