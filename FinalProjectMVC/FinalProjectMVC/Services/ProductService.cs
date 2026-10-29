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
            return products ?? new List<ProductVM>();
        }

        public async Task<ProductVM?> GetProductByIdAsync(int id)
        {
            return await _apiService.GetAsync<ProductVM>($"api/products/{id}");
        }

        public async Task<List<ProductVM>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await _apiService.GetAsync<List<ProductVM>>($"api/products/Category/{categoryId}");
            return products ?? new List<ProductVM>();
        }

        public async Task<ProductVM?> CreateProductAsync(CreateProductVM model)
        {
            return await _apiService.PostAsync<ProductVM>("api/products", model);
        }
    }
}
