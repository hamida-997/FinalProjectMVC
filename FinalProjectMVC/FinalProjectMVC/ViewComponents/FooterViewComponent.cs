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
                var active = (await _categoryService.GetAllCategoriesAsync())
                    .Where(c => c.IsActive)
                    .OrderBy(c => c.Name)
                    .ToList();
                model.Categories = active.Take(FooterVM.MaxCategories).ToList();
                model.HasMoreCategories = active.Count > FooterVM.MaxCategories;
            }
            catch
            {
                // The footer must never break a page; it just shows an "All products" link
            }

            return View(model);
        }
    }
}
