namespace FinalProjectMVC.Services.Interfaces
{
    public interface IApiService
    {
        Task<T?> GetAsync<T>(string endpoint);
        Task<T?> PostAsync<T>(string endpoint, object data);
        Task<T?> PostFormAsync<T>(string endpoint, MultipartFormDataContent content);
        Task<T?> PutAsync<T>(string endpoint, object data);
        Task<bool> DeleteAsync(string endpoint);

        // Same as above, but return the API's error message on failure
        Task<ApiResult<T>> GetWithResultAsync<T>(string endpoint);
        Task<ApiResult<T>> PostWithResultAsync<T>(string endpoint, object data);
        Task<ApiResult<T>> PutWithResultAsync<T>(string endpoint, object data);
        Task<ApiResult<object>> DeleteWithResultAsync(string endpoint);
    }
}
