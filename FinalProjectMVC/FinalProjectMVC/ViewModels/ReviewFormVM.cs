using System.ComponentModel.DataAnnotations;

namespace FinalProjectMVC.ViewModels
{
    // Posted by the review form on the product details page
    public class ReviewFormVM
    {
        [Range(1, int.MaxValue)]
        public int ProductId { get; set; }

        // Set when editing an existing review
        public int? ReviewId { get; set; }

        [Range(1, 5, ErrorMessage = "Please choose a rating from 1 to 5 stars.")]
        public int Rating { get; set; }

        [StringLength(1000, ErrorMessage = "Comment cannot exceed 1000 characters.")]
        public string? Comment { get; set; }
    }
}
