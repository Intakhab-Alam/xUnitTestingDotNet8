namespace ProductCatalog.API.Models
{
    public class Customer
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;  // Full name
        public string Email { get; set; } = null!;  // Unique email address

        // Navigation property
        public List<Order> Orders { get; set; } = new();
    }
}