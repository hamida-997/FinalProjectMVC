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
        public async Task<IActionResult> UpdateQuantity(int productId, int quantity)
        {
            if (quantity > 0)
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product != null && quantity > product.StockQuantity)
                {
                    // Don't let the cart ask for more than we have
                    quantity = Math.Max(product.StockQuantity, 0);
                    TempData["ErrorMessage"] = product.StockQuantity > 0
                        ? $"Only {product.StockQuantity} of '{product.Name}' in stock."
                        : $"'{product.Name}' is sold out.";
                }
            }

            _cartService.UpdateQuantity(productId, quantity);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            _cartService.RemoveFromCart(productId);
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            TempData["SuccessMessage"] = "Cart cleared successfully.";
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

            if (quantity < 1) quantity = 1;

            // Count what is already in the cart, otherwise adding twice could exceed the stock
            var inCart = _cartService.GetCartItems().FirstOrDefault(c => c.ProductId == productId)?.Quantity ?? 0;

            if (!product.IsAvailable || product.StockQuantity <= 0)
            {
                TempData["ErrorMessage"] = $"'{product.Name}' is not available right now.";
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            if (inCart + quantity > product.StockQuantity)
            {
                var canAdd = product.StockQuantity - inCart;
                TempData["ErrorMessage"] = canAdd > 0
                    ? $"Only {product.StockQuantity} of '{product.Name}' in stock and you already have {inCart} in your cart. You can add {canAdd} more."
                    : $"You already have all {product.StockQuantity} of '{product.Name}' in your cart.";
                return SafeRedirect(returnUrl) ?? RedirectToAction("Details", "Products", new { id = productId });
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

            return SafeRedirect(returnUrl) ?? RedirectToAction("Index", "Products");
        }

        private IActionResult? SafeRedirect(string? returnUrl) =>
            !string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl) ? Redirect(returnUrl) : null;
    }
}
