namespace FinalProjectMVC.ViewModels
{
    public class OrderVM
    {
        public int Id { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string CustomerEmail { get; set; } = string.Empty;
        public string CustomerPhone { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        // Subtotal before discount; older orders have Subtotal == TotalAmount
        public decimal Subtotal { get; set; }
        public decimal DiscountAmount { get; set; }
        public string? DiscountCode { get; set; }
        public bool HasDiscount => DiscountAmount > 0;

        public decimal TotalAmount { get; set; }
        public OrderStatus Status { get; set; }
        public string StatusName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public DateTime? DeliveryDate { get; set; }
        public string? Notes { get; set; }

        // Payment (API PaymentStatus enum: 0 Unpaid, 1 Paid, 2 Failed, 3 Refunded)
        public int PaymentStatus { get; set; }
        public string PaymentStatusName { get; set; } = "Unpaid";
        public DateTime? PaidAt { get; set; }
        public bool IsPaid => PaymentStatus == 1;

        public List<OrderItemVM> OrderItems { get; set; } = new List<OrderItemVM>();
    }
}
