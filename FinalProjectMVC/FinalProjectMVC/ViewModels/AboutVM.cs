namespace FinalProjectMVC.ViewModels
{
    public class AboutVM
    {
        public int ProductCount { get; set; }
        public int CategoryCount { get; set; }
        public int ReviewCount { get; set; }
        public double AverageRating { get; set; }

        public List<ProductVM> TopProducts { get; set; } = new();
        public List<CategoryVM> Categories { get; set; } = new();
        public List<ReviewVM> Reviews { get; set; } = new();
    }
}
