using FinalProjectMVC.Models;
using FinalProjectMVC.Services;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly IApiService _apiService;
        private readonly ILogger<FooterViewComponent> _logger;

        public FooterViewComponent(IApiService apiService, ILogger<FooterViewComponent> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new FooterViewModel();

            try
            {
                var categories = await _apiService.GetAsync<List<CategoryDto>>("api/categories");
                if (categories != null)
                {
                    model.Categories = categories.Where(c => c.IsActive).Take(5).ToList();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching categories for footer");
            }

            return View(model);
        }
    }
}
