using FinalProjectMVC.Models;
using FinalProjectMVC.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly IApiService _apiService;

        public FooterViewComponent(IApiService apiService)
        {
            _apiService = apiService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new FooterViewModel();

            try
            {
                // Fetch categories from API to show in footer
                var categories = await _apiService.GetAsync<List<CategoryDto>>("api/categories");
                if (categories != null)
                {
                    model.Categories = categories.Where(c => c.IsActive).Take(5).ToList();
                }
            }
            catch
            {
                // Silently fail - footer will show default categories
            }

            return View(model);
        }
    }
}
