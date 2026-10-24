namespace FinalProjectMVC.ViewModels
{
    public class ProductsListVM
    {
        public List<ProductVM> Products { get; set; } = new List<ProductVM>();
        public List<CategoryVM> Categories { get; set; } = new List<CategoryVM>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }
        public string? SortBy { get; set; }
    }
}
