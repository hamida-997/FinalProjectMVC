using System.ComponentModel.DataAnnotations;

namespace FinalProjectMVC.ViewModels
{
    public class UpdateOrderStatusVM
    {
        [Required]
        public OrderStatus Status { get; set; }

        [DataType(DataType.Date)]
        public DateTime? DeliveryDate { get; set; }
    }
}
