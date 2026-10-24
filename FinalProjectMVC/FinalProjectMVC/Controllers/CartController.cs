using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.Controllers
{
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly IProductService _productService;

        public CartController(ICartService cartService, IProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        public IActionResult Index()
        {
            var cartItems = _cartService.GetCartItems();
            return View(cartItems);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1, string? returnUrl = null)
        {
            var product = await _productService.GetProductByIdAsync(productId);

            if (product == null)
            {
                TempData["ErrorMessage"] = "Product not found.";
                return RedirectToAction("Index", "Products");
            }

            if (!product.IsAvailable || product.StockQuantity < quantity)
            {
                TempData["ErrorMessage"] = "Product is not available in the requested quantity.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Find primary image, fallback to first
            var primaryImage = product.Images.FirstOrDefault(i => i.IsPrimary);
            if (primaryImage == null)
            {
                primaryImage = product.Images.FirstOrDefault();
            }

            var cartItem = new CartItemVM
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Price = product.Price,
                Quantity = quantity,
                ImageUrl = primaryImage?.ImageUrl
            };

            _cartService.AddToCart(cartItem);

            TempData["SuccessMessage"] = $"'{product.Name}' added to cart!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Products");
        }
    }
}
