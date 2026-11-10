using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseVM?> RegisterAsync(RegisterVM model);
        Task<AuthResponseVM?> LoginAsync(LoginVM model);
        Task LogoutAsync();
        Task<ApiResult<AuthResponseVM>> ConfirmEmailAsync(string userId, string token);
        Task<ApiResult<object>> ResendConfirmationAsync(string email);

        // After a profile edit: update the name/phone/address shown in the header and checkout
        Task RefreshCurrentUserAsync(ProfileVM profile);
        bool IsAuthenticated();
        string? GetToken();
        UserInfoVM? GetCurrentUser();
    }
}
