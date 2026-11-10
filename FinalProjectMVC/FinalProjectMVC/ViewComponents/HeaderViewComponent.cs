using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly IAuthService _authService;
        private readonly ICartService _cartService;

        public HeaderViewComponent(IAuthService authService, ICartService cartService)
        {
            _authService = authService;
            _cartService = cartService;
        }

        public IViewComponentResult Invoke()
        {
            var currentUser = _authService.GetCurrentUser();

            var isAuthenticated = _authService.IsAuthenticated();

            var model = new HeaderVM
            {
                IsAuthenticated = isAuthenticated,
                UserName = currentUser?.UserName,
                FirstName = currentUser?.FirstName,
                LastName = currentUser?.LastName,
                Email = currentUser?.Email,
                CartItemCount = _cartService.GetCartItemCount(),
                // Role claims come from the auth cookie, the same check [Authorize(Roles=...)] uses
                IsAdmin = isAuthenticated &&
                    (UserClaimsPrincipal.IsInRole("Admin") || UserClaimsPrincipal.IsInRole("SuperAdmin"))
            };

            return View(model);
        }
    }
}
