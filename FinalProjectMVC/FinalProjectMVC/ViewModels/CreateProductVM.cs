using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FinalProjectMVC.ViewModels
{
    public class CreateProductVM
    {
        // Must match the API's FileUploadSettings
        public static readonly string[] AllowedImageExtensions = { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
        public const long MaxImageBytes = 5 * 1024 * 1024;
        public const int MaxImages = 4;

        [Required(ErrorMessage = "Product name is required.")]
        [StringLength(200, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 200 characters.")]
        [Display(Name = "Product Name")]
        public string Name { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters.")]
        [Display(Name = "Description")]
        public string? Description { get; set; }

        // Nullable so the field starts empty instead of showing "0"
        [Required(ErrorMessage = "Price is required.")]
        [Range(0.01, 100000, ErrorMessage = "Price must be between $0.01 and $100,000.")]
        [Display(Name = "Price")]
        public decimal? Price { get; set; }

        [Required(ErrorMessage = "Stock quantity is required.")]
        [Range(0, 100000, ErrorMessage = "Stock must be between 0 and 100,000.")]
        [Display(Name = "Stock Quantity")]
        public int? StockQuantity { get; set; }

        [Display(Name = "Available for Sale")]
        public bool IsAvailable { get; set; } = true;

        [Required(ErrorMessage = "Please select a category.")]
        [Range(1, int.MaxValue, ErrorMessage = "Please select a category.")]
        [Display(Name = "Category")]
        public int? CategoryId { get; set; }

        // Uploaded separately after the product is created; never sent in the product JSON.
        [ValidateNever]
        [JsonIgnore]
        [Display(Name = "Product Photos")]
        public List<IFormFile> Images { get; set; } = new();

        // Index into Images of the photo the admin picked as the main (cover) photo
        [JsonIgnore]
        public int PrimaryImageIndex { get; set; }
    }
}
