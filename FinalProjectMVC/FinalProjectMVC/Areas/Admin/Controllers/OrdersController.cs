using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Areas.Admin.Controllers
{
    [Area("Admin")]
    [Authorize(Roles = "Admin,SuperAdmin")]
    public class OrdersController : Controller
    {
        private readonly IOrderService _orderService;
        private readonly IAuthService _authService;

        public OrdersController(IOrderService orderService, IAuthService authService)
        {
            _orderService = orderService;
            _authService = authService;
        }

        public async Task<IActionResult> Index(OrderStatus? status, int page = 1)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            IEnumerable<OrderVM> orders = await _orderService.GetAllOrdersAsync();

            if (status.HasValue)
            {
                orders = orders.Where(o => o.Status == status.Value);
            }

            ViewBag.SelectedStatus = status;
            // Sort before paging so page 1 holds the newest orders
            return View(orders.OrderByDescending(o => o.OrderDate).Paginate(page, ViewData));
        }

        public async Task<IActionResult> Details(int id)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var order = await _orderService.GetOrderByIdAsync(id);
            if (order == null)
            {
                TempData["ErrorMessage"] = $"Order #{id} not found.";
                return RedirectToAction(nameof(Index));
            }

            return View(order);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, OrderStatus status, DateTime? deliveryDate)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var model = new UpdateOrderStatusVM
            {
                Status = status,
                DeliveryDate = deliveryDate
            };

            var result = await _orderService.UpdateOrderStatusAsync(id, model);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Order #{id} is now {OrderStatusText.Label(status)}.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to update order status.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancel(int id)
        {
            if (!HasAdminAccess())
            {
                return RedirectToLogin();
            }

            var result = await _orderService.CancelOrderAsync(id);

            if (result.Success)
            {
                TempData["SuccessMessage"] = $"Order #{id} cancelled.";
            }
            else
            {
                TempData["ErrorMessage"] = result.ErrorMessage ?? "Failed to cancel order. It may already be delivered or cancelled.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        private bool HasAdminAccess()
        {
            if (!_authService.IsAuthenticated())
            {
                return false;
            }

            var currentUser = _authService.GetCurrentUser();
            return currentUser != null &&
                (currentUser.Roles.Contains("SuperAdmin") || currentUser.Roles.Contains("Admin"));
        }

        private IActionResult RedirectToLogin()
        {
            return RedirectToAction("Login", "Auth", new { area = "" });
        }
    }
}
