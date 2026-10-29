using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IProductImageService
    {
        Task<List<ProductImageVM>> GetImagesByProductAsync(int productId);
        Task<ProductImageVM?> UploadImageAsync(int productId, IFormFile file, bool isPrimary);
        Task<bool> SetPrimaryAsync(int productId, int imageId);
        Task<bool> DeleteImageAsync(int id);
    }
}
