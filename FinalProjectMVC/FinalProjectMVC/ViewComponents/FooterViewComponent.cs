using FinalProjectMVC.Services.Interfaces;
using FinalProjectMVC.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace FinalProjectMVC.ViewComponents
{
    public class FooterViewComponent : ViewComponent
    {
        private readonly ICategoryService _categoryService;

        public FooterViewComponent(ICategoryService categoryService)
        {
            _categoryService = categoryService;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var model = new FooterVM();

            try
            {
                var categories = await _categoryService.GetAllCategoriesAsync();
                model.Categories = categories.Where(c => c.IsActive).Take(5).ToList();
            }
            catch
            {
                // Silently fail - footer will show default categories
            }

            return View(model);
        }
    }
}
