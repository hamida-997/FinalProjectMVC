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

        public Task<ApiResult<OrderVM>> UpdateOrderStatusAsync(int id, UpdateOrderStatusVM model)
        {
            return _apiService.PutWithResultAsync<OrderVM>($"api/orders/{id}/Status", model);
        }

        public Task<ApiResult<object>> CancelOrderAsync(int id)
        {
            return _apiService.PutWithResultAsync<object>($"api/orders/{id}/Cancel", new { });
        }
    }
}
