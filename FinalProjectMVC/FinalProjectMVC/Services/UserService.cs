using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using System.Text.Json;
using System.Text;

namespace FinalProjectMVC.Services
{
    public class UserService : IUserService
    {
        private readonly IApiService _apiService;

        public UserService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<UserVM>> GetAllUsersAsync(string? search = null, string? role = null)
        {
            var endpoint = "api/users";
            var queryParams = new List<string>();

            if (!string.IsNullOrWhiteSpace(search))
                queryParams.Add($"search={Uri.EscapeDataString(search)}");

            if (!string.IsNullOrWhiteSpace(role))
                queryParams.Add($"role={Uri.EscapeDataString(role)}");

            if (queryParams.Any())
                endpoint += "?" + string.Join("&", queryParams);

            var users = await _apiService.GetAsync<List<UserVM>>(endpoint);
            return users ?? new List<UserVM>();
        }

        public async Task<UserVM?> GetUserByIdAsync(string id)
        {
            return await _apiService.GetAsync<UserVM>($"api/users/{id}");
        }

        public Task<ApiResult<UserVM>> CreateUserAsync(CreateUserVM model)
        {
            var data = new
            {
                userName = model.UserName,
                email = model.Email,
                password = model.Password,
                firstName = model.FirstName,
                lastName = model.LastName,
                phoneNumber = model.PhoneNumber,
                address = model.Address,
                roles = model.Roles
            };

            return _apiService.PostWithResultAsync<UserVM>("api/users", data);
        }

        public Task<ApiResult<object>> UpdateUserAsync(string id, UpdateUserVM model)
        {
            var data = new
            {
                firstName = model.FirstName,
                lastName = model.LastName,
                phoneNumber = model.PhoneNumber,
                address = model.Address,
                isActive = model.IsActive,
                roles = model.Roles
            };

            return _apiService.PutWithResultAsync<object>($"api/users/{id}", data);
        }

        public Task<ApiResult<object>> DeleteUserAsync(string id)
        {
            return _apiService.DeleteWithResultAsync($"api/users/{id}");
        }

        public Task<ApiResult<object>> ChangePasswordAsync(string id, ChangePasswordVM model)
        {
            var data = new
            {
                userId = model.UserId,
                newPassword = model.NewPassword
            };

            return _apiService.PostWithResultAsync<object>($"api/users/{id}/change-password", data);
        }

        public Task<ApiResult<object>> ToggleUserStatusAsync(string id)
        {
            return _apiService.PostWithResultAsync<object>($"api/users/{id}/toggle-status", new { });
        }

        public async Task<List<string>> GetRolesAsync()
        {
            var roles = await _apiService.GetAsync<List<string>>("api/users/roles");
            return roles ?? new List<string>();
        }

        public async Task<Dictionary<string, int>> GetStatisticsAsync()
        {
            var stats = await _apiService.GetAsync<Dictionary<string, int>>("api/users/statistics");
            return stats ?? new Dictionary<string, int>();
        }
    }
}
