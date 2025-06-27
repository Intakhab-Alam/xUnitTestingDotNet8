using Microsoft.EntityFrameworkCore;
using ProductCatalog.API.Data;
using ProductCatalog.API.Models;
using ProductCatalog.API.Repositories;

namespace ProductCatalog.Tests.Repositories
{
    public class ProductRepositoryTests
    {
        // Helper method that creates a fresh, isolated ApplicationDbContext using EF Core InMemory provider
        private ApplicationDbContext GetInMemoryDbContext()
        {
            // Configure DbContextOptions to use an InMemory database with a unique name
            // Guid.NewGuid().ToString() ensures a new separate database per test, avoiding data conflicts
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: System.Guid.NewGuid().ToString())
                .Options;

            // Create a new ApplicationDbContext instance with the above options
            var context = new ApplicationDbContext(options);

            // Seed initial product data into the in-memory database for testing
            context.Products.AddRange(
                new Product { Id = 1, Name = "Test Laptop", Price = 60000m, Stock = 20 },       // Product 1
                new Product { Id = 2, Name = "Test Smartphone", Price = 25000m, Stock = 50 }     // Product 2
            );

            // Persist seeded data immediately to the in-memory store
            context.SaveChanges();

            // Return the prepared in-memory context for use in tests
            return context;
        }

        // Test method to verify GetByIdAsync returns a product when the product exists in the database
        [Fact]
        public async Task GetByIdAsync_ProductExists_ReturnsProduct()
        {
            // Arrange: create a fresh in-memory DbContext and instantiate ProductRepository
            var context = GetInMemoryDbContext();
            var repository = new ProductRepository(context);

            // Act: attempt to retrieve product with ID=1 (exists in seeded data)
            var product = await repository.GetByIdAsync(1);

            // Assert: product should not be null
            Assert.NotNull(product);

            // Assert: product's Name matches the seeded value "Test Laptop"
            Assert.Equal("Test Laptop", product.Name);

            // Assert: product's Price matches the seeded value 60000m
            Assert.Equal(60000m, product.Price);
        }

        // Test method to verify GetByIdAsync returns null when the product does not exist
        [Fact]
        public async Task GetByIdAsync_ProductDoesNotExist_ReturnsNull()
        {
            // Arrange: setup fresh context and repository
            var context = GetInMemoryDbContext();
            var repository = new ProductRepository(context);

            // Act: attempt to fetch a product with ID=999 which does not exist in seeded data
            var product = await repository.GetByIdAsync(999);

            // Assert: product should be null since it does not exist
            Assert.Null(product);
        }

        // Test method to verify AddAsync successfully adds a product to the database
        [Fact]
        public async Task AddAsync_ProductIsAdded_ProductExistsInDb()
        {
            // Arrange: setup fresh context and repository
            var context = GetInMemoryDbContext();
            var repository = new ProductRepository(context);

            // Create a new product instance to be added
            var newProduct = new Product
            {
                Id = 3,
                Name = "Test Tablet",
                Price = 15000m,
                Stock = 30
            };

            // Act: add new product asynchronously using repository method
            await repository.AddAsync(newProduct);

            // Save changes to persist new product in in-memory database
            await repository.SaveChangesAsync();

            // Assert: retrieve the newly added product by ID
            var addedProduct = await repository.GetByIdAsync(3);

            // Assert: product should not be null (exists)
            Assert.NotNull(addedProduct);

            // Assert: product Name should be "Test Tablet"
            Assert.Equal("Test Tablet", addedProduct.Name);
        }

        // Test method to verify changes to a product are persisted correctly after SaveChangesAsync
        [Fact]
        public async Task SaveChangesAsync_ModifiesData_DataIsPersisted()
        {
            // Arrange: setup fresh context and repository
            var context = GetInMemoryDbContext();
            var repository = new ProductRepository(context);

            // Act: fetch an existing product by ID=1
            var product = await repository.GetByIdAsync(1);

            // Assert: ensure product is not null before modifying it
            Assert.NotNull(product);

            // Modify the Stock property of the fetched product
            product!.Stock = 15;

            // Save changes to persist the modification in in-memory database
            await repository.SaveChangesAsync();

            // Assert: fetch the product again to verify changes persisted
            var updatedProduct = await repository.GetByIdAsync(1);

            // Assert: product still exists
            Assert.NotNull(updatedProduct);

            // Assert: Stock property reflects the updated value 15
            Assert.Equal(15, updatedProduct!.Stock);
        }
    }
}