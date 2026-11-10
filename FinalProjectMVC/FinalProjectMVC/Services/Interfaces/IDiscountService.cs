using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IDiscountService
    {
        // Admin
        Task<List<DiscountCodeVM>> GetAllAsync();
        Task<DiscountCodeVM?> GetByIdAsync(int id);
        Task<ApiResult<DiscountCodeVM>> CreateAsync(SaveDiscountCodeVM model);
        Task<ApiResult<DiscountCodeVM>> UpdateAsync(int id, SaveDiscountCodeVM model);
        Task<ApiResult<DiscountCodeVM>> ToggleActiveAsync(int id);
        Task<bool> DeleteAsync(int id);

        // Checkout preview: the API prices the cart items itself
        Task<ApiResult<DiscountPreviewVM>> ValidateAsync(string code, List<CartItemVM> items);
    }
}
