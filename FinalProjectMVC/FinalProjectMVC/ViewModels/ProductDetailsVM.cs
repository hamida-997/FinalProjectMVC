namespace FinalProjectMVC.ViewModels
{
    public class ProductDetailsVM
    {
        public ProductVM Product { get; set; } = new ProductVM();
        public List<ProductVM> RelatedProducts { get; set; } = new List<ProductVM>();

        public List<ReviewVM> Reviews { get; set; } = new List<ReviewVM>();

        // Null when the user isn't logged in
        public ReviewEligibilityVM? Eligibility { get; set; }

        // The logged-in user's own review, if they wrote one
        public ReviewVM? MyReview { get; set; }
    }
}
