using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class DiscountService : IDiscountService
    {
        private readonly IApiService _apiService;

        public DiscountService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<DiscountCodeVM>> GetAllAsync()
        {
            var codes = await _apiService.GetAsync<List<DiscountCodeVM>>("api/discounts");
            return codes ?? new List<DiscountCodeVM>();
        }

        public Task<DiscountCodeVM?> GetByIdAsync(int id)
        {
            return _apiService.GetAsync<DiscountCodeVM>($"api/discounts/{id}");
        }

        public Task<ApiResult<DiscountCodeVM>> CreateAsync(SaveDiscountCodeVM model)
        {
            return _apiService.PostWithResultAsync<DiscountCodeVM>("api/discounts", ToPayload(model));
        }

        public Task<ApiResult<DiscountCodeVM>> UpdateAsync(int id, SaveDiscountCodeVM model)
        {
            return _apiService.PutWithResultAsync<DiscountCodeVM>($"api/discounts/{id}", ToPayload(model));
        }

        // The API has no toggle endpoint, so send the current values with IsActive flipped
        public async Task<ApiResult<DiscountCodeVM>> ToggleActiveAsync(int id)
        {
            var current = await GetByIdAsync(id);
            if (current == null)
            {
                return ApiResult<DiscountCodeVM>.Fail("Discount code not found.", StatusCodes.Status404NotFound);
            }

            var payload = new
            {
                code = current.Code,
                description = current.Description,
                type = (int)current.Type,
                value = current.Value,
                minOrderAmount = current.MinOrderAmount,
                maxUses = current.MaxUses,
                expiresAt = current.ExpiresAt.HasValue
                    ? DateTime.SpecifyKind(current.ExpiresAt.Value, DateTimeKind.Utc)
                    : (DateTime?)null,
                isActive = !current.IsActive
            };

            return await _apiService.PutWithResultAsync<DiscountCodeVM>($"api/discounts/{id}", payload);
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _apiService.DeleteAsync($"api/discounts/{id}");
        }

        public Task<ApiResult<DiscountPreviewVM>> ValidateAsync(string code, List<CartItemVM> items)
        {
            return _apiService.PostWithResultAsync<DiscountPreviewVM>("api/discounts/validate", new
            {
                code,
                items = items.Select(i => new { productId = i.ProductId, quantity = i.Quantity })
            });
        }

        private static object ToPayload(SaveDiscountCodeVM model) => new
        {
            code = model.Code,
            description = model.Description,
            type = (int)model.Type,
            value = model.Value,
            minOrderAmount = model.MinOrderAmount,
            maxUses = model.MaxUses,
            // The form uses local time; the API stores UTC
            expiresAt = model.ExpiresAt.HasValue
                ? DateTime.SpecifyKind(model.ExpiresAt.Value, DateTimeKind.Local).ToUniversalTime()
                : (DateTime?)null,
            isActive = model.IsActive
        };
    }
}
