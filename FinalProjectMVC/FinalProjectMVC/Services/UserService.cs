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

        public async Task<UserVM?> CreateUserAsync(CreateUserVM model)
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

            return await _apiService.PostAsync<UserVM>("api/users", data);
        }

        public async Task<bool> UpdateUserAsync(string id, UpdateUserVM model)
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

            try
            {
                await _apiService.PutAsync<object>($"api/users/{id}", data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> DeleteUserAsync(string id)
        {
            return await _apiService.DeleteAsync($"api/users/{id}");
        }

        public async Task<bool> ChangePasswordAsync(string id, ChangePasswordVM model)
        {
            var data = new
            {
                userId = model.UserId,
                newPassword = model.NewPassword
            };

            try
            {
                await _apiService.PostAsync<object>($"api/users/{id}/change-password", data);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public async Task<bool> ToggleUserStatusAsync(string id)
        {
            try
            {
                await _apiService.PostAsync<object>($"api/users/{id}/toggle-status", new { });
                return true;
            }
            catch
            {
                return false;
            }
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
