using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class ProductImageService : IProductImageService
    {
        private readonly IApiService _apiService;

        public ProductImageService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ProductImageVM>> GetImagesByProductAsync(int productId)
        {
            var images = await _apiService.GetAsync<List<ProductImageVM>>($"api/productimages/Product/{productId}");
            return images ?? new List<ProductImageVM>();
        }

        public async Task<ProductImageVM?> UploadImageAsync(int productId, IFormFile file, bool isPrimary)
        {
            using var content = new MultipartFormDataContent();
            using var fileStream = file.OpenReadStream();
            var streamContent = new StreamContent(fileStream);
            content.Add(streamContent, "file", file.FileName);
            content.Add(new StringContent(productId.ToString()), "productId");
            content.Add(new StringContent(isPrimary.ToString().ToLower()), "isPrimary");

            return await _apiService.PostFormAsync<ProductImageVM>("api/productimages/Upload", content);
        }

        public async Task<bool> SetPrimaryAsync(int productId, int imageId)
        {
            var result = await _apiService.PutAsync<object>($"api/productimages/{imageId}/SetPrimary?productId={productId}", new { });
            return result != null;
        }

        public async Task<bool> DeleteImageAsync(int id)
        {
            return await _apiService.DeleteAsync($"api/productimages/{id}");
        }
    }
}
