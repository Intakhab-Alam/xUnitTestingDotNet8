using Microsoft.EntityFrameworkCore;
using ProductCatalog.API.Models;
using Microsoft.EntityFrameworkCore.Diagnostics;

namespace ProductCatalog.API.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Seed Products
            modelBuilder.Entity<Product>().HasData(
                new Product { Id = 1, Name = "Laptop", Price = 60000m, Stock = 20, Description = "High performance laptop" },
                new Product { Id = 2, Name = "Smartphone", Price = 25000m, Stock = 50, Description = "Latest smartphone" },
                new Product { Id = 3, Name = "Wireless Mouse", Price = 1500m, Stock = 100, Description = "Ergonomic wireless mouse" }
            );

            // Seed Customers
            modelBuilder.Entity<Customer>().HasData(
                new Customer { Id = 1, Name = "Intakhab Alam", Email = "intakhab@example.com" },
                new Customer { Id = 2, Name = "Sneha Das", Email = "sneha@example.com" }
            );
        }

        public DbSet<Product> Products { get; set; }
        public DbSet<Customer> Customers { get; set; }
        public DbSet<Order> Orders { get; set; }
        public DbSet<OrderItem> OrderItems { get; set; }
    }
}