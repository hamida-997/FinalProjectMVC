using FinalProjectMVC.Models;
using FinalProjectMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class AuthController : Controller
    {
        private readonly IApiService _apiService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IApiService apiService, ILogger<AuthController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Register()
        {
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

                if (!string.IsNullOrEmpty(response.Token))
                {
                    HttpContext.Session.SetString("AuthToken", response.Token);
                    HttpContext.Session.SetString("UserName", response.User?.UserName ?? string.Empty);
                    HttpContext.Session.SetString("UserEmail", response.User?.Email ?? string.Empty);
                }

                TempData["SuccessMessage"] = "Registration successful! Welcome to Caramellize.";
                return RedirectToAction("Index", "Home");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during registration");
                ModelState.AddModelError(string.Empty, "An unexpected error occurred. Please try again.");
                return View(model);
            }
        }
    }
}
