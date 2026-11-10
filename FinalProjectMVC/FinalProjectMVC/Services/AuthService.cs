using FinalProjectMVC.ViewModels;
using FinalProjectMVC.Services.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using System.Security.Claims;

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
        private const string UserPhoneKey = "UserPhone";
        private const string UserAddressKey = "UserAddress";

        public AuthService(IApiService apiService, IHttpContextAccessor httpContextAccessor)
        {
            _apiService = apiService;
            _httpContextAccessor = httpContextAccessor;
        }

        // Registration no longer logs the user in; they must confirm their email first
        public async Task<AuthResponseVM?> RegisterAsync(RegisterVM model)
        {
            var result = await _apiService.PostWithResultAsync<AuthResponseVM>("api/auth/Register", model);
            return ToResponse(result);
        }

        public async Task<AuthResponseVM?> LoginAsync(LoginVM model)
        {
            var result = await _apiService.PostWithResultAsync<AuthResponseVM>("api/auth/Login", model);
            var response = ToResponse(result);

            if (response != null && response.IsSuccess && !string.IsNullOrEmpty(response.Token))
            {
                await SaveUserSessionAsync(response);
            }

            return response;
        }

        public Task<ApiResult<AuthResponseVM>> ConfirmEmailAsync(string userId, string token)
        {
            var url = $"api/auth/confirm-email?userId={Uri.EscapeDataString(userId)}&token={Uri.EscapeDataString(token)}";
            return _apiService.GetWithResultAsync<AuthResponseVM>(url);
        }

        public Task<ApiResult<object>> ResendConfirmationAsync(string email)
        {
            return _apiService.PostWithResultAsync<object>("api/auth/resend-confirmation", new { email });
        }

        public async Task RefreshCurrentUserAsync(ProfileVM profile)
        {
            var httpContext = _httpContextAccessor.HttpContext;
            var current = httpContext?.User;
            if (httpContext == null || current?.Identity?.IsAuthenticated != true)
                return;

            httpContext.Session.SetString(UserFirstNameKey, profile.FirstName);
            httpContext.Session.SetString(UserLastNameKey, profile.LastName);
            httpContext.Session.SetString(UserPhoneKey, profile.PhoneNumber ?? string.Empty);
            httpContext.Session.SetString(UserAddressKey, profile.Address ?? string.Empty);

            // Rebuild the cookie with the new names; keep id, email and roles as they are
            var claims = current.Claims
                .Where(c => c.Type != ClaimTypes.GivenName && c.Type != ClaimTypes.Surname)
                .ToList();
            claims.Add(new Claim(ClaimTypes.GivenName, profile.FirstName));
            claims.Add(new Claim(ClaimTypes.Surname, profile.LastName));

            var auth = await httpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme)),
                auth.Properties ?? new AuthenticationProperties { IsPersistent = true });
        }

        // null = API unreachable; otherwise a response carrying the API's message
        private static AuthResponseVM? ToResponse(ApiResult<AuthResponseVM> result)
        {
            if (result.Success)
                return result.Data;

            if (result.StatusCode == 0)
                return null;

            return new AuthResponseVM
            {
                IsSuccess = false,
                Message = result.ErrorMessage ?? "The request could not be completed.",
                EmailNotConfirmed = result.StatusCode == StatusCodes.Status403Forbidden
            };
        }

        public async Task LogoutAsync()
        {
            _httpContextAccessor.HttpContext?.Session.Clear();
            
            if (_httpContextAccessor.HttpContext != null)
            {
                await _httpContextAccessor.HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            }
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
            var user = _httpContextAccessor.HttpContext?.User;
            if (user == null || !user.Identity?.IsAuthenticated == true)
            {
                return null;
            }

            var roles = user.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            return new UserInfoVM
            {
                Id = user.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? string.Empty,
                UserName = user.FindFirst(ClaimTypes.Name)?.Value ?? string.Empty,
                Email = user.FindFirst(ClaimTypes.Email)?.Value ?? string.Empty,
                FirstName = user.FindFirst(ClaimTypes.GivenName)?.Value ?? string.Empty,
                LastName = user.FindFirst(ClaimTypes.Surname)?.Value ?? string.Empty,
                PhoneNumber = _httpContextAccessor.HttpContext?.Session.GetString(UserPhoneKey),
                Address = _httpContextAccessor.HttpContext?.Session.GetString(UserAddressKey),
                Roles = roles
            };
        }

        private async Task SaveUserSessionAsync(AuthResponseVM response)
        {
            var session = _httpContextAccessor.HttpContext?.Session;
            var httpContext = _httpContextAccessor.HttpContext;
            
            if (session == null || httpContext == null || response.User == null) 
                return;

            // Save to session (for API token)
            session.SetString(AuthTokenKey, response.Token ?? string.Empty);
            session.SetString(UserIdKey, response.User.Id);
            session.SetString(UserNameKey, response.User.UserName);
            session.SetString(UserEmailKey, response.User.Email);
            session.SetString(UserFirstNameKey, response.User.FirstName);
            session.SetString(UserLastNameKey, response.User.LastName);
            session.SetString(UserPhoneKey, response.User.PhoneNumber ?? string.Empty);
            session.SetString(UserAddressKey, response.User.Address ?? string.Empty);

            if (response.ExpiresAt.HasValue)
            {
                session.SetString(TokenExpiresAtKey, response.ExpiresAt.Value.ToString("O"));
            }

            // Create claims for cookie authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, response.User.Id),
                new Claim(ClaimTypes.Name, response.User.UserName),
                new Claim(ClaimTypes.Email, response.User.Email),
                new Claim(ClaimTypes.GivenName, response.User.FirstName),
                new Claim(ClaimTypes.Surname, response.User.LastName)
            };

            // Add role claims
            if (response.User.Roles != null)
            {
                foreach (var role in response.User.Roles)
                {
                    claims.Add(new Claim(ClaimTypes.Role, role));
                }
            }

            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true,
                ExpiresUtc = response.ExpiresAt ?? DateTimeOffset.UtcNow.AddHours(1)
            };

            await httpContext.SignInAsync(
                CookieAuthenticationDefaults.AuthenticationScheme,
                new ClaimsPrincipal(claimsIdentity),
                authProperties);
        }
    }
}
