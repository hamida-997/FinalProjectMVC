using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface ICategoryService
    {
        Task<List<CategoryVM>> GetAllCategoriesAsync();
        Task<CategoryVM?> GetCategoryByIdAsync(int id);
        Task<CategoryVM?> CreateCategoryAsync(CreateCategoryVM model);
    }
}
