using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseVM?> RegisterAsync(RegisterVM model);
        Task<AuthResponseVM?> LoginAsync(LoginVM model);
        void Logout();
        bool IsAuthenticated();
        string? GetToken();
        UserInfoVM? GetCurrentUser();
    }
}
