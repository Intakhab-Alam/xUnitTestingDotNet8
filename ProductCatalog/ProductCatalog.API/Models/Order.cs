using System.ComponentModel.DataAnnotations.Schema;
namespace ProductCatalog.API.Models
{
    public class Order
    {
        public int Id { get; set; }
        public int CustomerId { get; set; }
        public Customer? Customer { get; set; }
        public DateTime OrderDate { get; set; } // UTC date/time

        [Column(TypeName = "decimal(12,2)")]
        public decimal BaseAmount { get; set; } // Sum of all item totals before discount

        [Column(TypeName = "decimal(12,2)")]
        public decimal DiscountAmount { get; set; }  // Total discount at order level

        [Column(TypeName = "decimal(12,2)")]
        public decimal TotalAmount { get; set; }  // Final amount after discount

        // Navigation property
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}