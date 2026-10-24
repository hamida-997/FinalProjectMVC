using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface ICartService
    {
        List<CartItemVM> GetCartItems();
        void AddToCart(CartItemVM item);
        void UpdateQuantity(int productId, int quantity);
        void RemoveFromCart(int productId);
        void ClearCart();
        int GetCartItemCount();
        decimal GetCartTotal();
    }
}
