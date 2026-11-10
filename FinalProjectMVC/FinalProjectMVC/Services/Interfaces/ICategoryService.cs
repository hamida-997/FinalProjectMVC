using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryVM>> GetAllCategoriesAsync();
        Task<CategoryVM?> GetCategoryByIdAsync(int id);
        Task<ApiResult<CategoryVM>> CreateCategoryAsync(CreateCategoryVM model);
        Task<ApiResult<CategoryVM>> UpdateCategoryAsync(int id, UpdateCategoryVM model);
        Task<ApiResult<object>> DeleteCategoryAsync(int id);
    }
}
