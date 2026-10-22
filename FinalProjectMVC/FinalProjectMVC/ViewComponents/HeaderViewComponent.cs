using FinalProjectMVC.Models;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.ViewComponents
{
    public class HeaderViewComponent : ViewComponent
    {
        public IViewComponentResult Invoke()
        {
            var session = HttpContext.Session;
            var isAuthenticated = !string.IsNullOrEmpty(session.GetString("AuthToken"));

            var model = new HeaderViewModel
            {
                IsAuthenticated = isAuthenticated,
                UserName = session.GetString("UserName"),
                FirstName = session.GetString("UserFirstName"),
                Email = session.GetString("UserEmail"),
                CartItemCount = 0 
            };

            return View(model);
        }
    }
}
