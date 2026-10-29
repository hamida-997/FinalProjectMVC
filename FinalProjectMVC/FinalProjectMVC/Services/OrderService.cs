using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class OrderService : IOrderService
    {
        private readonly IApiService _apiService;

        public OrderService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<OrderVM>> GetAllOrdersAsync()
        {
            var orders = await _apiService.GetAsync<List<OrderVM>>("api/orders");
            return orders ?? new List<OrderVM>();
        }

        public async Task<OrderVM?> GetOrderByIdAsync(int id)
        {
            return await _apiService.GetAsync<OrderVM>($"api/orders/{id}");
        }

        public async Task<OrderVM?> UpdateOrderStatusAsync(int id, UpdateOrderStatusVM model)
        {
            return await _apiService.PutAsync<OrderVM>($"api/orders/{id}/Status", model);
        }

        public async Task<bool> CancelOrderAsync(int id)
        {
            var result = await _apiService.PutAsync<object>($"api/orders/{id}/Cancel", new { });
            return result != null;
        }
    }
}
