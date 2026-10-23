namespace FinalProjectMVC.ViewModels
{
    public class ProductDetailsVM
    {
        public ProductVM Product { get; set; } = new ProductVM();
        public List<ProductVM> RelatedProducts { get; set; } = new List<ProductVM>();
    }
}
