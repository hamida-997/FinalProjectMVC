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

            var model = new HeaderVM
            {
                IsAuthenticated = _authService.IsAuthenticated(),
                UserName = currentUser?.UserName,
                FirstName = currentUser?.FirstName,
                Email = currentUser?.Email,
                CartItemCount = _cartService.GetCartItemCount()
            };

            return View(model);
        }
    }
}
