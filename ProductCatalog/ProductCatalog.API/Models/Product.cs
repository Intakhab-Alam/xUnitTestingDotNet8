using System.ComponentModel.DataAnnotations.Schema;
namespace ProductCatalog.API.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; } = string.Empty; // Product Name
        [Column(TypeName = "decimal(12,2)")]
        public decimal Price { get; set; } // Current price
        public int Stock { get; set; } // Available stock for orders
        public string? Description { get; set; }  // Description for catalog, marketing, etc.

        // Navigation property
        public List<OrderItem> OrderItems { get; set; } = new();
    }
}