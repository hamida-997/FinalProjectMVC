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
    }
}
