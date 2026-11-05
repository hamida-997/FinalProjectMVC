namespace FinalProjectMVC.ViewModels
{
    // Response of POST api/payments/checkout
    public class CheckoutSessionVM
    {
        public int OrderId { get; set; }
        public string SessionId { get; set; } = string.Empty;
        public string Url { get; set; } = string.Empty;
    }
}
