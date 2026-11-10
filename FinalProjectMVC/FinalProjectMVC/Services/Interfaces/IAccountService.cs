using FinalProjectMVC.ViewModels;

namespace FinalProjectMVC.Services.Interfaces
{
    public interface IAccountService
    {
        Task<ApiResult<ProfileVM>> GetProfileAsync();
        Task<ApiResult<ProfileVM>> UpdateProfileAsync(EditProfileVM model);
        Task<ApiResult<object>> ChangePasswordAsync(ChangeOwnPasswordVM model);
        Task<ApiResult<List<OrderVM>>> GetMyOrdersAsync();
        Task<ApiResult<OrderVM>> GetMyOrderAsync(int id);
        Task<ApiResult<object>> CancelMyOrderAsync(int id);
    }
}
