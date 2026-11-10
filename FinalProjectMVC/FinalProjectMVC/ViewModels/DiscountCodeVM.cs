using System.ComponentModel.DataAnnotations;

namespace FinalProjectMVC.ViewModels
{
    // Matches the API's DiscountType enum
    public enum DiscountTypeVM
    {
        [Display(Name = "Percentage (%)")]
        Percentage = 0,

        [Display(Name = "Fixed amount ($)")]
        FixedAmount = 1
    }

    // Matches the API's DiscountCodeDto
    public class DiscountCodeVM
    {
        public int Id { get; set; }
        public string Code { get; set; } = string.Empty;
        public string? Description { get; set; }
        public DiscountTypeVM Type { get; set; }
        public decimal Value { get; set; }
        public decimal? MinOrderAmount { get; set; }
        public int? MaxUses { get; set; }
        public int UsedCount { get; set; }
        public DateTime? ExpiresAt { get; set; }
        public bool IsActive { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
        public bool IsUsable { get; set; }

        public string ValueDisplay => Type == DiscountTypeVM.Percentage
            ? $"{Value:0.##}% off"
            : $"${Value:F2} off";

        public bool IsExpired => ExpiresAt.HasValue && ExpiresAt.Value <= DateTime.UtcNow;
        public bool IsUsedUp => MaxUses.HasValue && UsedCount >= MaxUses.Value;
    }

    // Admin create/edit form
    public class SaveDiscountCodeVM
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Code is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Code must be between 3 and 50 characters")]
        [RegularExpression(@"^\s*[A-Za-z0-9_-]+\s*$", ErrorMessage = "Code can only contain letters, numbers, - and _")]
        [Display(Name = "Code")]
        public string Code { get; set; } = string.Empty;

        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        [Display(Name = "Description (optional)")]
        public string? Description { get; set; }

        [Display(Name = "Discount Type")]
        public DiscountTypeVM Type { get; set; } = DiscountTypeVM.Percentage;

        [Required(ErrorMessage = "Value is required")]
        [Range(0.01, 100000, ErrorMessage = "Value must be greater than 0")]
        [Display(Name = "Value")]
        public decimal Value { get; set; }

        [Range(0, 100000, ErrorMessage = "Minimum order cannot be negative")]
        [Display(Name = "Minimum Order ($, optional)")]
        public decimal? MinOrderAmount { get; set; }

        [Range(1, int.MaxValue, ErrorMessage = "Max uses must be at least 1")]
        [Display(Name = "Max Uses (optional)")]
        public int? MaxUses { get; set; }

        // Entered in the admin's local time; converted to UTC before sending to the API
        [DataType(DataType.DateTime)]
        [Display(Name = "Expires At (optional)")]
        public DateTime? ExpiresAt { get; set; }

        [Display(Name = "Active")]
        public bool IsActive { get; set; } = true;

        // Display only on the edit page
        public int UsedCount { get; set; }
    }
}
