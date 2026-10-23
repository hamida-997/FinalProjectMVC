using FinalProjectMVC.ViewModels;
using FinalProjectMVC.Services.Interfaces;

namespace FinalProjectMVC.Services
{
    public class AuthService : IAuthService
    {
        private readonly IApiService _apiService;
        private readonly IHttpContextAccessor _httpContextAccessor;

        // Session keys
        private const string AuthTokenKey = "AuthToken";
        private const string UserIdKey = "UserId";
        private const string UserNameKey = "UserName";
        private const string UserEmailKey = "UserEmail";
        private const string UserFirstNameKey = "UserFirstName";
        private const string UserLastNameKey = "UserLastName";
        private const string TokenExpiresAtKey = "TokenExpiresAt";

        public AuthService(IApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<AuthResponseVM?> RegisterAsync(RegisterVM model)
        {
            var response = await _apiService.PostAsync<AuthResponseVM>("api/auth/Register", model);

            if (response != null && response.IsSuccess)
            {
                SaveUserSession(response);
            }

            return response;
        }

        public async Task<AuthResponseVM?> LoginAsync(LoginVM model)
        {
            var response = await _apiService.PostAsync<AuthResponseVM>("api/auth/Login", model);

            if (response != null && response.IsSuccess)
            {
                SaveUserSession(response);
            }

            return response;
        }

        public void Logout()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
        }

        public bool IsAuthenticated()
        {
            return !string.IsNullOrEmpty(GetToken());
        }

        public string? GetToken()
        {
            return _httpContextAccessor.HttpContext?.Session.GetString(AuthTokenKey);
        }

        public UserInfoVM? GetCurrentUser()
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null || string.IsNullOrEmpty(session.GetString(AuthTokenKey)))
            {
                return null;
            }

            return new UserInfoVM
            {
                Id = session.GetString(UserIdKey) ?? string.Empty,
                UserName = session.GetString(UserNameKey) ?? string.Empty,
                Email = session.GetString(UserEmailKey) ?? string.Empty,
                FirstName = session.GetString(UserFirstNameKey) ?? string.Empty,
                LastName = session.GetString(UserLastNameKey) ?? string.Empty
            };
        }

        private void SaveUserSession(AuthResponseVM response)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            if (session == null || response.User == null) return;

            session.SetString(AuthTokenKey, response.Token ?? string.Empty);
            session.SetString(UserIdKey, response.User.Id);
            session.SetString(UserNameKey, response.User.UserName);
            session.SetString(UserEmailKey, response.User.Email);
            session.SetString(UserFirstNameKey, response.User.FirstName);
            session.SetString(UserLastNameKey, response.User.LastName);

            if (response.ExpiresAt.HasValue)
            {
                session.SetString(TokenExpiresAtKey, response.ExpiresAt.Value.ToString("O"));
            }
        }
    }
}
