using FinalProjectMVC.Models;
using FinalProjectMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;

        public AuthController(IApiService apiService)
        {
            _apiService = apiService;
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Redirect if already logged in
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.PostAsync<AuthResponseViewModel>("api/auth/Register", model);

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

                // Store token and user info in session
                if (!string.IsNullOrEmpty(response.Token))
                {
                    SaveUserSession(response);
                }

                TempData["SuccessMessage"] = "Registration successful! Welcome to Caramellize.";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Login()
        {
            // Redirect if already logged in
            if (!string.IsNullOrEmpty(HttpContext.Session.GetString("AuthToken")))
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var response = await _apiService.PostAsync<AuthResponseViewModel>("api/auth/Login", model);

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

                // Store token and user info in session
                if (!string.IsNullOrEmpty(response.Token))
                {
                    SaveUserSession(response);
                }

                TempData["SuccessMessage"] = $"Welcome back, {response.User?.FirstName}!";
                return RedirectToAction("Index", "Home");
            }
            catch
            {
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }

        [HttpGet]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            return RedirectToAction("Index", "Home");
        }

        private void SaveUserSession(AuthResponseViewModel response)
        {
            HttpContext.Session.SetString("AuthToken", response.Token ?? string.Empty);
            HttpContext.Session.SetString("UserName", response.User?.UserName ?? string.Empty);
            HttpContext.Session.SetString("UserEmail", response.User?.Email ?? string.Empty);
            HttpContext.Session.SetString("UserFirstName", response.User?.FirstName ?? string.Empty);
            HttpContext.Session.SetString("UserId", response.User?.Id ?? string.Empty);

            if (response.ExpiresAt.HasValue)
            {
                HttpContext.Session.SetString("TokenExpiresAt", response.ExpiresAt.Value.ToString("O"));
            }
        }
    }
}
