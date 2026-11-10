using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly IApiService _apiService;

        public CategoryService(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<List<CategoryVM>> GetAllCategoriesAsync()
        {
            var categories = await _apiService.GetAsync<List<CategoryVM>>("api/categories");
            return categories ?? new List<CategoryVM>();
        }

        public async Task<CategoryVM?> GetCategoryByIdAsync(int id)
        {
            return await _apiService.GetAsync<CategoryVM>($"api/categories/{id}");
        }

        public Task<ApiResult<CategoryVM>> CreateCategoryAsync(CreateCategoryVM model)
        {
            return _apiService.PostWithResultAsync<CategoryVM>("api/categories", model);
        }

        public Task<ApiResult<CategoryVM>> UpdateCategoryAsync(int id, UpdateCategoryVM model)
        {
            return _apiService.PutWithResultAsync<CategoryVM>($"api/categories/{id}", model);
        }

        public Task<ApiResult<object>> DeleteCategoryAsync(int id)
        {
            return _apiService.DeleteWithResultAsync($"api/categories/{id}");
        }
    }
}
