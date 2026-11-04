using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IUserService
    {
        Task<List<UserVM>> GetAllUsersAsync(string? search = null, string? role = null);
        Task<UserVM?> GetUserByIdAsync(string id);
        Task<UserVM?> CreateUserAsync(CreateUserVM model);
        Task<bool> UpdateUserAsync(string id, UpdateUserVM model);
        Task<bool> DeleteUserAsync(string id);
        Task<bool> ChangePasswordAsync(string id, ChangePasswordVM model);
        Task<bool> ToggleUserStatusAsync(string id);
        Task<List<string>> GetRolesAsync();
        Task<Dictionary<string, int>> GetStatisticsAsync();
    }
}
