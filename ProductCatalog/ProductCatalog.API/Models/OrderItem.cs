using System.ComponentModel.DataAnnotations.Schema;
namespace ProductCatalog.API.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order? Order { get; set; }
        public int ProductId { get; set; }
        public Product? Product { get; set; }
        public int Quantity { get; set; } // Units ordered
        [Column(TypeName = "decimal(12,2)")]
        public decimal UnitPrice { get; set; } // Price at time of order
        [Column(TypeName = "decimal(12,2)")]
        public decimal LineTotal { get; set; }  // Calculated: UnitPrice * Quantity
    }
}