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
        private readonly IDiscountService _discountService;

        public CheckoutController(ICartService cartService, IAuthService authService,
            IPaymentService paymentService, IDiscountService discountService)
        {
            _cartService = cartService;
            _authService = authService;
            _paymentService = paymentService;
            _discountService = discountService;
        }

        // GET: /Checkout?code=SWEET10  (code is optional; the cart page's promo box sends it)
        [HttpGet]
        public async Task<IActionResult> Index(string? code)
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

            if (!string.IsNullOrWhiteSpace(code))
            {
                model.DiscountCode = code.Trim();
                await LoadDiscountAsync(model);
            }

            return View(model);
        }

        // POST: /Checkout/ApplyDiscount  (same form as PlaceOrder, so typed details are kept)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ApplyDiscount(CheckoutVM model)
        {
            if (!PrepareModel(model))
            {
                return RedirectToAction("Index", "Cart");
            }

            // Don't show errors for delivery fields the customer hasn't finished yet
            ModelState.Clear();

            if (string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                ModelState.AddModelError(nameof(model.DiscountCode), "Please enter a discount code.");
            }
            else
            {
                await LoadDiscountAsync(model);
            }

            return View("Index", model);
        }

        // POST: /Checkout/RemoveDiscount
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveDiscount(CheckoutVM model)
        {
            if (!PrepareModel(model))
            {
                return RedirectToAction("Index", "Cart");
            }

            // Clearing ModelState also makes the code input render empty
            ModelState.Clear();
            model.DiscountCode = null;
            model.Discount = null;

            return View("Index", model);
        }

        // POST: /Checkout/PlaceOrder
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PlaceOrder(CheckoutVM model)
        {
            if (!PrepareModel(model))
            {
                return RedirectToAction("Index", "Cart");
            }

            // Re-check the code so the summary shows the discounted total, and stop here
            // with a clear message if it's no longer valid (e.g. expired since it was applied)
            if (!string.IsNullOrWhiteSpace(model.DiscountCode))
            {
                await LoadDiscountAsync(model);
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            // The API validates the code again and sets the price; this preview is only for display
            var result = await _paymentService.StartCheckoutAsync(model, model.Items);

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

        // GET: /Checkout/Success?session_id=cs_test_...
        // Stripe redirects here after payment. The API verifies the session with Stripe,
        // so opening this URL by hand does not mark anything as paid.
        [HttpGet]
        public async Task<IActionResult> Success([FromQuery(Name = "session_id")] string? sessionId)
        {
            if (string.IsNullOrWhiteSpace(sessionId))
            {
                return RedirectToAction("Index", "Cart");
            }

            var result = await _paymentService.ConfirmPaymentAsync(sessionId);

            if (result.StatusCode == StatusCodes.Status401Unauthorized)
            {
                // Token expired while paying: log in again, then come back and confirm
                await _authService.LogoutAsync();
                TempData["ErrorMessage"] = "Your session has expired. Please log in to see your order.";
                return RedirectToAction("Login", "Auth",
                    new { returnUrl = Url.Action("Success", "Checkout", new { session_id = sessionId }) });
            }

            if (!result.Success || result.Data == null)
            {
                // Payment not complete or couldn't be verified. Keep the cart.
                ViewData["ErrorMessage"] = result.ErrorMessage ?? "We couldn't confirm your payment.";
                return View("PaymentProblem");
            }

            // Safe to repeat on refresh: confirm is idempotent and clearing an empty cart is a no-op
            _cartService.ClearCart();

            return View(result.Data);
        }

        // GET: /Checkout/Cancel
        // Stripe redirects here when the customer clicks "Back" on the payment page.
        [HttpGet]
        public IActionResult Cancel()
        {
            return View();
        }

        // Always use the server-side cart and account email, never values posted by the form.
        // Returns false when the cart is empty.
        private bool PrepareModel(CheckoutVM model)
        {
            var items = _cartService.GetCartItems();
            if (!items.Any())
            {
                TempData["ErrorMessage"] = "Your cart is empty.";
                return false;
            }

            model.Items = items;
            model.CustomerEmail = _authService.GetCurrentUser()?.Email ?? string.Empty;
            model.Discount = null;
            return true;
        }

        // Asks the API what the code is worth for this cart. On failure, adds the reason
        // as an error on the code field.
        private async Task LoadDiscountAsync(CheckoutVM model)
        {
            var result = await _discountService.ValidateAsync(model.DiscountCode!.Trim(), model.Items);

            if (!result.Success || result.Data == null)
            {
                ModelState.AddModelError(nameof(model.DiscountCode), result.ErrorMessage ?? "The discount code could not be checked.");
                return;
            }

            if (!result.Data.IsValid)
            {
                ModelState.AddModelError(nameof(model.DiscountCode), result.Data.Message ?? "This discount code is not valid.");
                return;
            }

            model.Discount = result.Data;
            model.DiscountCode = result.Data.Code;
        }
    }
}
