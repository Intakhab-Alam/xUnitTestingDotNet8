using System.ComponentModel.DataAnnotations;
namespace ProductCatalog.API.DTOs
{
    public class OrderCreateDTO
    {
        [Required(ErrorMessage = "CustomerId is required.")]
        public int CustomerId { get; set; }
        [Required(ErrorMessage = "Order must contain at least one item.")]
        [MinLength(1, ErrorMessage = "Order must have at least one item.")]
        public List<OrderItemDTO> Items { get; set; } = new();
    }
}