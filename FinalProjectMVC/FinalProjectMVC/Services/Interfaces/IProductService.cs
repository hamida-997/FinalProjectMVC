using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductVM>> GetAllProductsAsync();
        Task<ProductVM?> GetProductByIdAsync(int id);
        Task<List<ProductVM>> GetProductsByCategoryAsync(int categoryId);
        Task<ProductVM?> CreateProductAsync(CreateProductVM model);
    }
}
