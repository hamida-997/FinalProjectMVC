using FinalProjectMVC.ViewModels;
using FinalProjectMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        private readonly IAuthService _authService;

        public HeaderViewComponent(IAuthService authService)
        {
            _authService = authService;
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
                CartItemCount = 0 // TODO: Get actual cart count later
            };

            return View(model);
        }
    }
}
