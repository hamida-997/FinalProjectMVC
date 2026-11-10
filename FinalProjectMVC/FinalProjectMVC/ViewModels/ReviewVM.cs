namespace FinalProjectMVC.ViewModels
{
    // Matches the API's ReviewDto
    public class ReviewVM
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public string ReviewerName { get; set; } = string.Empty;
        public int Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    // Matches the API's ReviewEligibilityDto
    public class ReviewEligibilityVM
    {
        public bool HasPurchased { get; set; }
        public int? ExistingReviewId { get; set; }
        public bool CanReview { get; set; }
    }
}
