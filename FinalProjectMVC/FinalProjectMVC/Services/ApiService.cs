using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using FinalProjectMVC.Services.Interfaces;

namespace FinalProjectMVC.Services
{
    public class ApiService : IApiService
    {
        private readonly HttpClient _httpClient;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly JsonSerializerOptions _jsonOptions;

        private const string AuthTokenSessionKey = "AuthToken";

        public ApiService(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
        {
            _httpClient = httpClient;
            _httpContextAccessor = httpContextAccessor;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<T?> GetAsync<T>(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync(endpoint);
                response.EnsureSuccessStatusCode();

                var content = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(content, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> PostAsync<T>(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> PostFormAsync<T>(string endpoint, MultipartFormDataContent content)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.PostAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public async Task<T?> PutAsync<T>(string endpoint, object data)
        {
            try
            {
                AddAuthorizationHeader();
                var json = JsonSerializer.Serialize(data);
                var content = new StringContent(json, Encoding.UTF8, "application/json");

                var response = await _httpClient.PutAsync(endpoint, content);
                response.EnsureSuccessStatusCode();

                var responseContent = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<T>(responseContent, _jsonOptions);
            }
            catch
            {
                return default;
            }
        }

        public async Task<bool> DeleteAsync(string endpoint)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(endpoint);
                return response.IsSuccessStatusCode;
            }
            catch
            {
                return false;
            }
        }

        public Task<ApiResult<T>> GetWithResultAsync<T>(string endpoint) =>
            SendWithResultAsync<T>(HttpMethod.Get, endpoint, null);

        public Task<ApiResult<T>> PostWithResultAsync<T>(string endpoint, object data) =>
            SendWithResultAsync<T>(HttpMethod.Post, endpoint, data);

        public Task<ApiResult<T>> PutWithResultAsync<T>(string endpoint, object data) =>
            SendWithResultAsync<T>(HttpMethod.Put, endpoint, data);

        private async Task<ApiResult<T>> SendWithResultAsync<T>(HttpMethod method, string endpoint, object? data)
        {
            HttpResponseMessage response;
            try
            {
                AddAuthorizationHeader();
                using var request = new HttpRequestMessage(method, endpoint);
                if (data != null)
                {
                    request.Content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                }
                response = await _httpClient.SendAsync(request);
            }
            catch (HttpRequestException)
            {
                return ApiResult<T>.Fail("Unable to connect to the server. Please try again later.", 0);
            }
            catch (TaskCanceledException)
            {
                return ApiResult<T>.Fail("The server took too long to respond. Please try again.", 0);
            }

            using (response)
            {
                var statusCode = (int)response.StatusCode;
                var body = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    return ApiResult<T>.Fail(ExtractErrorMessage(body, statusCode), statusCode);
                }

                // 204 No Content and other empty bodies are still a success
                if (string.IsNullOrWhiteSpace(body))
                {
                    return ApiResult<T>.Ok(default, statusCode);
                }

                try
                {
                    return ApiResult<T>.Ok(JsonSerializer.Deserialize<T>(body, _jsonOptions), statusCode);
                }
                catch (JsonException)
                {
                    return ApiResult<T>.Fail("The server returned an unexpected response.", statusCode);
                }
            }
        }

        // Reads the API's error formats: { message }, validation { errors: { field: [..] } },
        // Identity { errors: [ { description } ] }, or ProblemDetails { title }.
        private static string ExtractErrorMessage(string body, int statusCode)
        {
            if (!string.IsNullOrWhiteSpace(body))
            {
                try
                {
                    using var doc = JsonDocument.Parse(body);
                    var root = doc.RootElement;

                    if (root.ValueKind == JsonValueKind.Object)
                    {
                        if (root.TryGetProperty("message", out var message) && message.ValueKind == JsonValueKind.String)
                        {
                            return message.GetString()!;
                        }

                        if (root.TryGetProperty("errors", out var errors))
                        {
                            var messages = new List<string>();

                            if (errors.ValueKind == JsonValueKind.Object)
                            {
                                foreach (var field in errors.EnumerateObject())
                                {
                                    if (field.Value.ValueKind == JsonValueKind.Array)
                                    {
                                        messages.AddRange(field.Value.EnumerateArray()
                                            .Where(e => e.ValueKind == JsonValueKind.String)
                                            .Select(e => e.GetString()!));
                                    }
                                }
                            }
                            else if (errors.ValueKind == JsonValueKind.Array)
                            {
                                foreach (var error in errors.EnumerateArray())
                                {
                                    if (error.ValueKind == JsonValueKind.Object &&
                                        error.TryGetProperty("description", out var description))
                                    {
                                        messages.Add(description.GetString() ?? string.Empty);
                                    }
                                }
                            }

                            if (messages.Count > 0)
                            {
                                return string.Join(" ", messages);
                            }
                        }

                        if (root.TryGetProperty("title", out var title) && title.ValueKind == JsonValueKind.String)
                        {
                            return title.GetString()!;
                        }
                    }
                }
                catch (JsonException)
                {
                    // Not JSON; fall through to the generic message
                }
            }

            return statusCode switch
            {
                401 => "Your session has expired. Please log in again.",
                403 => "You don't have permission to do that.",
                404 => "The requested item was not found.",
                >= 500 => "The server ran into a problem. Please try again later.",
                _ => "The request could not be completed."
            };
        }

        private void AddAuthorizationHeader()
        {
            var token = _httpContextAccessor.HttpContext?.Session.GetString(AuthTokenSessionKey);

            if (!string.IsNullOrEmpty(token))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);
            }
            else
            {
                _httpClient.DefaultRequestHeaders.Authorization = null;
            }
        }
    }
}
