using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.ModelBinding.Validation;

namespace FinalProjectMVC.ViewModels
{
    // Rules match the API's CreateOrderDto so errors are caught before calling it
    public class CheckoutVM
    {
        [Required(ErrorMessage = "Full name is required")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Name must be between 2 and 100 characters")]
        [Display(Name = "Full Name")]
        public string CustomerName { get; set; } = string.Empty;

        // Shown read-only; the server always uses the logged-in account's email
        [Display(Name = "Email")]
        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Please enter a valid phone number")]
        [StringLength(20, ErrorMessage = "Phone number cannot exceed 20 characters")]
        [Display(Name = "Phone Number")]
        public string CustomerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Delivery address is required")]
        [StringLength(500, MinimumLength = 10, ErrorMessage = "Address must be between 10 and 500 characters")]
        [Display(Name = "Delivery Address")]
        public string ShippingAddress { get; set; } = string.Empty;

        [StringLength(1000, ErrorMessage = "Notes cannot exceed 1000 characters")]
        [Display(Name = "Order Notes (optional)")]
        public string? Notes { get; set; }

        // Only the code text is posted; the API decides the discount amount
        [StringLength(50, ErrorMessage = "Discount code cannot exceed 50 characters")]
        [Display(Name = "Discount code")]
        public string? DiscountCode { get; set; }

        // Display only: always reloaded from the session cart, never trusted from the form
        [ValidateNever]
        public List<CartItemVM> Items { get; set; } = new();

        // Display only: set after the API accepts the code
        [ValidateNever]
        public DiscountPreviewVM? Discount { get; set; }

        public decimal Subtotal => Discount?.Subtotal ?? Items.Sum(i => i.Price * i.Quantity);
        public decimal Total => Discount?.Total ?? Subtotal;
    }

    // Matches the API's DiscountValidationResultDto
    public class DiscountPreviewVM
    {
        public bool IsValid { get; set; }
        public string? Message { get; set; }
        public string? Code { get; set; }
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public decimal Total { get; set; }
    }
}
