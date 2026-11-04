using System.ComponentModel.DataAnnotations;

namespace FinalProjectMVC.ViewModels
{
    public class UpdateUserVM
    {
        public string Id { get; set; } = string.Empty;

        [Required(ErrorMessage = "First name is required")]
        [StringLength(50, ErrorMessage = "First name cannot exceed 50 characters")]
        public string FirstName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Last name is required")]
        [StringLength(50, ErrorMessage = "Last name cannot exceed 50 characters")]
        public string LastName { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Invalid phone number")]
        public string? PhoneNumber { get; set; }

        public string? Address { get; set; }

        [Required(ErrorMessage = "Status is required")]
        public bool IsActive { get; set; }

        [Required(ErrorMessage = "At least one role is required")]
        public List<string> Roles { get; set; } = new List<string>();
    }
}
