using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserVM>> GetAllUsersAsync(string? search = null, string? role = null);
        Task<UserVM?> GetUserByIdAsync(string id);
        Task<ApiResult<UserVM>> CreateUserAsync(CreateUserVM model);
        Task<ApiResult<object>> UpdateUserAsync(string id, UpdateUserVM model);
        Task<ApiResult<object>> DeleteUserAsync(string id);
        Task<ApiResult<object>> ChangePasswordAsync(string id, ChangePasswordVM model);
        Task<ApiResult<object>> ToggleUserStatusAsync(string id);
        Task<List<string>> GetRolesAsync();
        Task<Dictionary<string, int>> GetStatisticsAsync();
    }
}
