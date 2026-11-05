using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    // Logged-out users are sent to /Auth/Login?ReturnUrl=/Checkout and come back here after login
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IAuthService _authService;
        private readonly IPaymentService _paymentService;

        public CheckoutController(ICartService cartService, IAuthService authService, IPaymentService paymentService)
        {
            _cartService = cartService;
            _authService = authService;
            _paymentService = paymentService;
        }

        // GET: /Checkout
        [HttpGet]
        public IActionResult Index()
        {
            var items = _cartService.GetCartItems();
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            var user = _authService.GetCurrentUser();

            var model = new CheckoutVM
            {
                CustomerName = $"{user?.FirstName} {user?.LastName}".Trim(),
                CustomerEmail = user?.Email ?? string.Empty,
                CustomerPhone = user?.PhoneNumber ?? string.Empty,
                ShippingAddress = user?.Address ?? string.Empty,
                Items = items
            };

            return View(model);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            // Always use the server-side cart and account email, never values posted by the form
            var items = _cartService.GetCartItems();
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return RedirectToAction("Index", "Cart");
            }

            model.Items = items;
            model.CustomerEmail = _authService.GetCurrentUser()?.Email ?? string.Empty;

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            var result = await _paymentService.StartCheckoutAsync(model, items);

            if (!result.Success || result.Data == null)
            {
                if (result.StatusCode == StatusCodes.Status401Unauthorized)
                {
                    // API token expired: log in again and come back to checkout
                    await _authService.LogoutAsync();
                    TempData["ErrorMessage"] = "Your session has expired. Please log in again.";
                    return RedirectToAction("Login", "Auth", new { returnUrl = Url.Action("Index", "Checkout") });
                }

                // e.g. "Insufficient stock for 'Chocolate Cake'..." — keep the cart so the user can fix it
                ModelState.AddModelError(string.Empty, result.ErrorMessage ?? "Your order could not be placed.");
                return View("Index", model);
            }

            // Only follow Stripe-hosted URLs, never an arbitrary redirect
            if (!Uri.TryCreate(result.Data.Url, UriKind.Absolute, out var paymentUrl)
                || paymentUrl.Scheme != Uri.UriSchemeHttps
                || paymentUrl.Host != "checkout.stripe.com")
            {
                ModelState.AddModelError(string.Empty, "Payment could not be started. Please try again.");
                return View("Index", model);
            }

            // The cart is cleared on the success page, after payment is confirmed
            return Redirect(paymentUrl.ToString());
        }
    }
}
