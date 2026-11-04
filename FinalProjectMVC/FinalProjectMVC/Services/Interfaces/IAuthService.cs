using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseVM?> RegisterAsync(RegisterVM model);
        Task<AuthResponseVM?> LoginAsync(LoginVM model);
        Task LogoutAsync();
        bool IsAuthenticated();
        string? GetToken();
        UserInfoVM? GetCurrentUser();
    }
}
