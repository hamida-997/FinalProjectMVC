using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IProductService
    {
        Task<List<ProductVM>> GetAllProductsAsync();
        Task<ProductVM?> GetProductByIdAsync(int id);
        Task<List<ProductVM>> GetProductsByCategoryAsync(int categoryId);
        Task<ApiResult<ProductVM>> CreateProductAsync(CreateProductVM model);
        Task<ApiResult<ProductVM>> UpdateProductAsync(int id, UpdateProductVM model);
        Task<ApiResult<object>> DeleteProductAsync(int id);
    }
}
