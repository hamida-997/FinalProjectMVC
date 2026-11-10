using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly IApiService _apiService;

        public PaymentService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<ApiResult<CheckoutSessionVM>> StartCheckoutAsync(CheckoutVM model, List<CartItemVM> items)
        {
            // Only product IDs and quantities are sent; the API prices the order itself
            var data = new
            {
                customerName = model.CustomerName,
                customerEmail = model.CustomerEmail,
                customerPhone = model.CustomerPhone,
                shippingAddress = model.ShippingAddress,
                notes = model.Notes,
                discountCode = string.IsNullOrWhiteSpace(model.DiscountCode) ? null : model.DiscountCode.Trim(),
                orderItems = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity })
            };

            return _apiService.PostWithResultAsync<CheckoutSessionVM>("api/payments/checkout", data);
        }

        public Task<ApiResult<OrderVM>> ConfirmPaymentAsync(string sessionId)
        {
            return _apiService.PostWithResultAsync<OrderVM>("api/payments/confirm", new { sessionId });
        }
    }
}
