namespace FinalProjectMVC.ViewModels
{
    public class HeaderVM
    {
        public bool IsAuthenticated { get; set; }
        public string? UserName { get; set; }
        public string? FirstName { get; set; }
        public string? Email { get; set; }
        public int CartItemCount { get; set; }
    }
}
