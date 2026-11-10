using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class AccountService : IAccountService
    {
        private readonly IApiService _apiService;

        public AccountService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public Task<ApiResult<ProfileVM>> GetProfileAsync() =>
            _apiService.GetWithResultAsync<ProfileVM>("api/account/me");

        public Task<ApiResult<ProfileVM>> UpdateProfileAsync(EditProfileVM model) =>
            _apiService.PutWithResultAsync<ProfileVM>("api/account/me", new
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                phoneNumber = model.PhoneNumber,
                address = model.Address
            });

        public Task<ApiResult<object>> ChangePasswordAsync(ChangeOwnPasswordVM model) =>
            _apiService.PostWithResultAsync<object>("api/account/change-password", new
            {
                currentPassword = model.CurrentPassword,
                newPassword = model.NewPassword
            });

        public Task<ApiResult<List<OrderVM>>> GetMyOrdersAsync() =>
            _apiService.GetWithResultAsync<List<OrderVM>>("api/account/orders");

        // The API only returns the order if it belongs to the logged-in user
        public Task<ApiResult<OrderVM>> GetMyOrderAsync(int id) =>
            _apiService.GetWithResultAsync<OrderVM>($"api/orders/{id}");

        public Task<ApiResult<object>> CancelMyOrderAsync(int id) =>
            _apiService.PutWithResultAsync<object>($"api/orders/{id}/Cancel", new { });
    }
}
