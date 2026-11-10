namespace FinalProjectMVC.ViewModels
{
    public class HeaderVM
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public string? Email { get; set; }
        public int CartItemCount { get; set; }

        // Admins and SuperAdmins get a link to the admin panel
        public bool IsAdmin { get; set; }

        // First name, else username, else email, so the menu never shows an empty name
        public string DisplayName =>
            !string.IsNullOrWhiteSpace(FirstName) ? FirstName :
            !string.IsNullOrWhiteSpace(UserName) ? UserName :
            !string.IsNullOrWhiteSpace(Email) ? Email : "Account";

        public string FullName => $"{FirstName} {LastName}".Trim() is { Length: > 0 } n ? n : DisplayName;

        public string Initial => DisplayName[..1].ToUpperInvariant();
    }
}
