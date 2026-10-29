namespace FinalProjectMVC.ViewModels
{
    public class ProductImagesVM
    {
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public List<ProductImageVM> Images { get; set; } = new List<ProductImageVM>();
    }
}
