using System.Text.RegularExpressions;

namespace FinalProjectMVC.ViewModels
{
    public static class OrderStatusText
    {
        // "ReadyForPickup" -> "Ready For Pickup"
        public static string Label(OrderStatus status) =>
            Regex.Replace(status.ToString(), "(?<!^)([A-Z])", " $1");
    }
}
