using FinalProjectMVC.Helpers;
using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class ProductService : IProductService
    {
        private readonly IApiService _apiService;

        public ProductService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<ProductVM>> GetAllProductsAsync()
        {
            var products = await _apiService.GetAsync<List<ProductVM>>("api/products");
            return ImageUrlHelper.Normalize(products ?? new List<ProductVM>());
        }

        public async Task<ProductVM?> GetProductByIdAsync(int id)
        {
            return ImageUrlHelper.Normalize(await _apiService.GetAsync<ProductVM>($"api/products/{id}"));
        }

        public async Task<List<ProductVM>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _apiService.GetAsync<List<ProductVM>>($"api/products/Category/{categoryId}");
            return ImageUrlHelper.Normalize(products ?? new List<ProductVM>());
        }

        public Task<ApiResult<ProductVM>> CreateProductAsync(CreateProductVM model)
        {
            // Only product fields; images are uploaded afterwards
            return _apiService.PostWithResultAsync<ProductVM>("api/products", new
            {
                name = model.Name,
                description = model.Description,
                price = model.Price ?? 0,
                stockQuantity = model.StockQuantity ?? 0,
                isAvailable = model.IsAvailable,
                categoryId = model.CategoryId ?? 0
            });
        }

        public Task<ApiResult<ProductVM>> UpdateProductAsync(int id, UpdateProductVM model)
        {
            return _apiService.PutWithResultAsync<ProductVM>($"api/products/{id}", model);
        }

        public Task<ApiResult<object>> DeleteProductAsync(int id)
        {
            return _apiService.DeleteWithResultAsync($"api/products/{id}");
        }
    }
}
