using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IPaymentService
    {
        // Creates an unpaid order and a Stripe Checkout session for it
        Task<ApiResult<CheckoutSessionVM>> StartCheckoutAsync(CheckoutVM model, List<CartItemVM> items);

        // Asks the API to verify the Stripe session and mark the order paid
        Task<ApiResult<OrderVM>> ConfirmPaymentAsync(string sessionId);
    }
}
