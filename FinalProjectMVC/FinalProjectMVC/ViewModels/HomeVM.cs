namespace FinalProjectMVC.ViewModels
{
    public class HomeVM
    {
        // Best-rated, then newest, available products
        public List<ProductVM> FeaturedProducts { get; set; } = new();

        // Products customers can actually buy (available, in an active category)
        public int ProductCount { get; set; }

        // Real customer reviews for the "What Our Customers Say" section
        public List<ReviewVM> Testimonials { get; set; } = new();
    }
}
