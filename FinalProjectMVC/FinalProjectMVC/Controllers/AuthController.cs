using FinalProjectMVC.ViewModels;
using FinalProjectMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterVM model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _authService.RegisterAsync(model);

                if (response == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to connect to the server. Please try again later.");
                    return View(model);
                }

                if (!response.IsSuccess)
                {
                    ModelState.AddModelError(string.Empty, response.Message);
                    return View(model);
                }

                // Not logged in yet: show "check your inbox"
                TempData["CheckEmailMessage"] = response.Message;
                return RedirectToAction(nameof(CheckEmail), new { email = model.Email });
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            if (_authService.IsAuthenticated())
            {
                return RedirectToAction("Index", "Home");
            }
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginVM model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _authService.LoginAsync(model);

                if (response == null)
                {
                    ModelState.AddModelError(string.Empty, "Unable to connect to the server. Please try again later.");
                    return View(model);
                }

                if (!response.IsSuccess)
                {
                    // The login view shows a "resend confirmation email" link for this case
                    ViewData["EmailNotConfirmed"] = response.EmailNotConfirmed;
                    ModelState.AddModelError(string.Empty, response.Message);
                    return View(model);
                }

                TempData["SuccessMessage"] = $"Welcome back, {response.User?.FirstName}!";

                // Only follow local URLs to prevent open redirects
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        // GET: /Auth/CheckEmail?email=...  (shown after registering)
        [HttpGet]
        public IActionResult CheckEmail(string? email)
        {
            ViewData["Email"] = email;
            ViewData["Message"] = TempData["CheckEmailMessage"] as string;
            return View();
        }

        // GET: /Auth/ConfirmEmail?userId=...&token=...  (the link in the email)
        [HttpGet]
        public async Task<IActionResult> ConfirmEmail(string? userId, string? token)
        {
            if (string.IsNullOrWhiteSpace(userId) || string.IsNullOrWhiteSpace(token))
            {
                ViewData["Success"] = false;
                ViewData["Message"] = "This confirmation link is incomplete. Please use the link from your email.";
                return View();
            }

            var result = await _authService.ConfirmEmailAsync(userId, token);

            ViewData["Success"] = result.Success;
            ViewData["Message"] = result.Success
                ? result.Data?.Message ?? "Your email is confirmed. You can now log in."
                : result.ErrorMessage ?? "This confirmation link is invalid or has expired.";
            return View();
        }

        // GET: /Auth/ResendConfirmation
        [HttpGet]
        public IActionResult ResendConfirmation(string? email)
        {
            ViewData["Email"] = email;
            return View();
        }

        // POST: /Auth/ResendConfirmation  (same URL as the GET, different C# name)
        [HttpPost]
        [ActionName("ResendConfirmation")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResendConfirmationPost(string email)
        {
            if (string.IsNullOrWhiteSpace(email))
            {
                ViewData["Error"] = "Please enter your email address.";
                return View();
            }

            var result = await _authService.ResendConfirmationAsync(email.Trim());
            if (!result.Success)
            {
                ViewData["Email"] = email;
                ViewData["Error"] = result.ErrorMessage ?? "The email could not be sent. Please try again.";
                return View();
            }

            TempData["CheckEmailMessage"] = "If that email belongs to an unconfirmed account, we've sent a new confirmation link.";
            return RedirectToAction(nameof(CheckEmail), new { email = email.Trim() });
        }

        // POST only, with an anti-forgery token: as a GET link, any other site could log
        // people out just by embedding <img src="/Auth/Logout">
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _authService.LogoutAsync();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        // Old bookmarks / typed URLs: don't log out, just go home
        [HttpGet]
        [ActionName("Logout")]
        public IActionResult LogoutGet() => RedirectToAction("Index", "Home");
    }
}
