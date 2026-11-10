using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IOrderService
    {
        Task<List<OrderVM>> GetAllOrdersAsync();
        Task<OrderVM?> GetOrderByIdAsync(int id);
        Task<ApiResult<OrderVM>> UpdateOrderStatusAsync(int id, UpdateOrderStatusVM model);
        Task<ApiResult<object>> CancelOrderAsync(int id);
    }
}
