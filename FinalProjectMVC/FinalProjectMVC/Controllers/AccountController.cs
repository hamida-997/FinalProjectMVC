using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    // "My Profile" and "My Orders" for the logged-in customer
    [Authorize]
    public class AccountController : Controller
    {
        private const string ErrorKey = "AccountError";
        private readonly IAccountService _accountService;
        private readonly IAuthService _authService;

        public AccountController(IAccountService accountService, IAuthService authService)
        {
            _accountService = accountService;
            _authService = authService;
        }

        // GET: /Account/Profile
        [HttpGet]
        public async Task<IActionResult> Profile()
        {
            var result = await _accountService.GetProfileAsync();
            if (IsSessionExpired(result)) return await SessionExpiredAsync(nameof(Profile));

            if (!result.Success || result.Data == null)
            {
                ViewData["Error"] = result.ErrorMessage ?? "Your profile could not be loaded.";
                return View(new ProfilePageVM());
            }

            return View(await BuildPageAsync(result.Data));
        }

        // POST: /Account/Profile
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Profile([Bind(Prefix = "Edit")] EditProfileVM model)
        {
            // Only validate the profile form, not the password form on the same page
            foreach (var key in ModelState.Keys.Where(k => !k.StartsWith("Edit.")).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                return await RedisplayAsync(edit: model);
            }

            var result = await _accountService.UpdateProfileAsync(model);
            if (IsSessionExpired(result)) return await SessionExpiredAsync(nameof(Profile));

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError("Edit", result.ErrorMessage ?? "Your profile could not be saved.");
                return await RedisplayAsync(edit: model);
            }

            await _authService.RefreshCurrentUserAsync(result.Data);
            TempData["SuccessMessage"] = "Your profile has been updated.";
            return RedirectToAction(nameof(Profile));
        }

        // POST: /Account/ChangePassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ChangePassword([Bind(Prefix = "Password")] ChangeOwnPasswordVM model)
        {
            foreach (var key in ModelState.Keys.Where(k => !k.StartsWith("Password.")).ToList())
                ModelState.Remove(key);

            if (!ModelState.IsValid)
            {
                return await RedisplayAsync(password: new ChangeOwnPasswordVM(), showPassword: true);
            }

            var result = await _accountService.ChangePasswordAsync(model);
            if (IsSessionExpired(result)) return await SessionExpiredAsync(nameof(Profile));

            if (!result.Success)
            {
                // e.g. "Incorrect password."
                ModelState.AddModelError("Password", result.ErrorMessage ?? "Your password could not be changed.");
                return await RedisplayAsync(password: new ChangeOwnPasswordVM(), showPassword: true);
            }

            TempData["SuccessMessage"] = "Your password has been changed.";
            return RedirectToAction(nameof(Profile));
        }

        // GET: /Account/Orders
        [HttpGet]
        public async Task<IActionResult> Orders(int page = 1)
        {
            var result = await _accountService.GetMyOrdersAsync();
            if (IsSessionExpired(result)) return await SessionExpiredAsync(nameof(Orders));

            if (!result.Success)
            {
                ViewData["Error"] = result.ErrorMessage ?? "Your orders could not be loaded.";
            }

            ViewData["Error"] ??= TempData[ErrorKey] as string;
            var orders = (result.Data ?? new List<OrderVM>()).OrderByDescending(o => o.OrderDate);
            return View(orders.Paginate(page, ViewData));
        }

        // GET: /Account/OrderDetails/5
        [HttpGet]
        public async Task<IActionResult> OrderDetails(int id)
        {
            var result = await _accountService.GetMyOrderAsync(id);
            if (IsSessionExpired(result)) return await SessionExpiredAsync(nameof(Orders));

            if (!result.Success || result.Data == null)
            {
                TempData[ErrorKey] = "That order could not be found.";
                return RedirectToAction(nameof(Orders));
            }

            ViewData["Error"] = TempData[ErrorKey] as string;
            return View(result.Data);
        }

        // POST: /Account/CancelOrder/5  (only unpaid, pending orders)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelOrder(int id)
        {
            var order = await _accountService.GetMyOrderAsync(id);
            if (IsSessionExpired(order)) return await SessionExpiredAsync(nameof(Orders));

            if (!order.Success || order.Data == null)
            {
                TempData[ErrorKey] = "That order could not be found.";
                return RedirectToAction(nameof(Orders));
            }

            // Paid orders need a refund, so customers can't cancel them here
            if (!CanCancel(order.Data))
            {
                TempData[ErrorKey] = "This order can no longer be cancelled online. Please contact us.";
                return RedirectToAction(nameof(OrderDetails), new { id });
            }

            var result = await _accountService.CancelMyOrderAsync(id);
            if (result.Success)
                TempData["SuccessMessage"] = $"Order #{id} has been cancelled.";
            else
                TempData[ErrorKey] = result.ErrorMessage ?? "The order could not be cancelled.";

            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        public static bool CanCancel(OrderVM order) =>
            order.Status == OrderStatus.Pending && !order.IsPaid;

        private async Task<ProfilePageVM> BuildPageAsync(ProfileVM profile)
        {
            var orders = await _accountService.GetMyOrdersAsync();
            return new ProfilePageVM
            {
                Profile = profile,
                Edit = new EditProfileVM
                {
                    FirstName = profile.FirstName,
                    LastName = profile.LastName,
                    PhoneNumber = profile.PhoneNumber,
                    Address = profile.Address
                },
                OrderCount = orders.Data?.Count ?? 0
            };
        }

        private async Task<IActionResult> RedisplayAsync(EditProfileVM? edit = null, ChangeOwnPasswordVM? password = null, bool showPassword = false)
        {
            var result = await _accountService.GetProfileAsync();
            var page = result.Data != null ? await BuildPageAsync(result.Data) : new ProfilePageVM();
            if (edit != null) page.Edit = edit;
            if (password != null) page.Password = password;
            ViewData["ShowPassword"] = showPassword;
            return View(nameof(Profile), page);
        }

        private static bool IsSessionExpired<T>(ApiResult<T> result) =>
            result.StatusCode == StatusCodes.Status401Unauthorized;

        private async Task<IActionResult> SessionExpiredAsync(string action)
        {
            await _authService.LogoutAsync();
            return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action(action, "Account") });
        }
    }
}
