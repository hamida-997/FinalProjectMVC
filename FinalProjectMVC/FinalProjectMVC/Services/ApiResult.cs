namespace FinalProjectMVC.Services
{
    /// <summary>
    /// Result of an API call that keeps the API's error message instead of
    /// collapsing every failure into null.
    /// </summary>
    public class ApiResult<T>
    {
        public bool Success { get; init; }
        public T? Data { get; init; }
        public string? ErrorMessage { get; init; }

        // 0 when the API could not be reached at all
        public int StatusCode { get; init; }

        public static ApiResult<T> Ok(T? data, int statusCode) =>
            new() { Success = true, Data = data, StatusCode = statusCode };

        public static ApiResult<T> Fail(string message, int statusCode) =>
            new() { Success = false, ErrorMessage = message, StatusCode = statusCode };
    }
}
