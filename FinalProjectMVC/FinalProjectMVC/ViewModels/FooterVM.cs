namespace FinalProjectMVC.ViewModels
{
    public class FooterVM
    {
        public const int MaxCategories = 5;

        public List<CategoryVM> Categories { get; set; } = new List<CategoryVM>();

        // More active categories exist than are listed, so show a "View all" link
        public bool HasMoreCategories { get; set; }
    }
}
