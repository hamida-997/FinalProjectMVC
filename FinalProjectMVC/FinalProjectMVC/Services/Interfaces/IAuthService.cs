using FinalProjectMVC.Models;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseViewModel?> RegisterAsync(RegisterViewModel model);
        Task<AuthResponseViewModel?> LoginAsync(LoginViewModel model);
        void Logout();
        bool IsAuthenticated();
        string? GetToken();
        UserInfoViewModel? GetCurrentUser();
    }
}
