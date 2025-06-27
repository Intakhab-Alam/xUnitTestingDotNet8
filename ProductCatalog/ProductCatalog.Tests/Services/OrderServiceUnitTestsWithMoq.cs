using Moq;
using ProductCatalog.API.DTOs;
using ProductCatalog.API.Exceptions;
using ProductCatalog.API.Models;
using ProductCatalog.API.Repositories;
using ProductCatalog.API.Services;
using Xunit;
namespace ProductCatalog.Tests.Services
{
    public class OrderServiceUnitTestsWithMoq
    {
        // Declare private fields for each mock repository interface:
        private readonly Mock<IOrderRepository> _mockOrderRepo;
        private readonly Mock<IProductRepository> _mockProductRepo;
        private readonly Mock<ICustomerRepository> _mockCustomerRepo;
        // Finally, we declare the IOrderService under test. We will pass mock.Object instances into it.
        private readonly IOrderService _orderService;
        // Constructor runs before every [Fact], setting up fresh mocks + service:
        public OrderServiceUnitTestsWithMoq()
        {
            // Create mock instances for each repository interface:
            _mockOrderRepo = new Mock<IOrderRepository>();
            _mockProductRepo = new Mock<IProductRepository>();
            _mockCustomerRepo = new Mock<ICustomerRepository>();
            // Finally, create the OrderService under test, injecting the mocks’ .Object properties
            //    - mockOrderRepo.Object is the actual fake IOrderRepository
            //    - mockProductRepo.Object is the fake IProductRepository
            //    - mockCustomerRepo.Object is the fake ICustomerRepository
            _orderService = new OrderService(
            _mockOrderRepo.Object,
            _mockProductRepo.Object,
            _mockCustomerRepo.Object);
        }
        // Unit Test for Successful Order Creation
        [Fact]
        public async Task CreateOrderAsync_WithValidInput_ReturnsOrderResponse()
        {
            // Define a valid customerId we will use for this test:
            int customerId = 1;
            // Build an OrderCreateDTO with two items:
            //    - Item 1: ProductId=10, Quantity=2
            //    - Item 2: ProductId=20, Quantity=1
            var orderDto = new OrderCreateDTO
            {
                CustomerId = customerId,
                Items = new List<OrderItemDTO>
                {
                    new() { ProductId = 10, Quantity = 2 },
                    new() { ProductId = 20, Quantity = 1 }
                }
            };
            // Setup the customer repository mock to return a valid Customer when GetByIdAsync(customerId) is called:
            _mockCustomerRepo
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(new Customer { Id = customerId, Name = "Test Customer" });
            // Setup the product repository mock for ProductId=10:
            //     - When GetByIdAsync(10) is called, return a Product with Stock=5, Price=50
            _mockProductRepo
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Product { Id = 10, Name = "Product10", Price = 50, Stock = 5 });
            // Setup the product repository mock for ProductId=20 similarly:
            _mockProductRepo
            .Setup(r => r.GetByIdAsync(20))
            .ReturnsAsync(new Product { Id = 20, Name = "Product20", Price = 100, Stock = 3 });
            // Setup the order repository mock’s AddAsync to do nothing (complete the Task) but be marked Verifiable
            // Whenever AddAsync is called with any Order object (doesn’t matter which one), just return a completed Task.
            // In Verify, it checks "was this method called with any Order?"(regardless of the actual order details).
            _mockOrderRepo
            .Setup(r => r.AddAsync(It.IsAny<Order>()))
            .Returns(Task.CompletedTask)
            .Verifiable(); // we will Verify() later that AddAsync was called once

            // Act: Call the service’s CreateOrderAsync
            var result = await _orderService.CreateOrderAsync(orderDto);

            // Assert: The returned DTO should not be null (order was created successfully)
            Assert.NotNull(result);
            // Assert: The CustomerId in the result should equal the one we passed (1)
            Assert.Equal(customerId, result.CustomerId);
            // Assert: The service should return 2 order items (we passed two items)
            Assert.Equal(2, result.OrderItems.Count);
            
            // Verify that AddAsync(Order) was called exactly once on the mock order repository
            _mockOrderRepo.Verify(r => r.AddAsync(It.IsAny<Order>()), Times.Once);
        }

        // Unit Test for Customer Not Found
        [Fact]
        public async Task CreateOrderAsync_CustomerNotFound_ThrowsNotFoundException()
        {
            // Setup CustomerRepository.GetByIdAsync(any int) to always return null (no such customer)
            // This will return null regardless of what CustomerId you ask for.
            // It's useful for "simulate not found" or "simulate error" cases.
            _mockCustomerRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Customer?)null);
            // Build a DTO with CustomerId=999, which our mock will treat as Not Found
            var orderDto = new OrderCreateDTO
            {
                CustomerId = 999,
                Items = new List<OrderItemDTO> { new() { ProductId = 1, Quantity = 1 } }
            };
            // Act & Assert: calling CreateOrderAsync should immediately throw NotFoundException
            await Assert.ThrowsAsync<NotFoundException>(() =>
            _orderService.CreateOrderAsync(orderDto)
            );
        }
        // Unit Test for Product Not Found
        [Fact]
        public async Task CreateOrderAsync_ProductNotFound_ThrowsNotFoundException()
        {
            // Define a valid customerId and have the customer mock return a real Customer
            int customerId = 1;
            _mockCustomerRepo
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(new Customer { Id = customerId });
            // Setup ProductRepository.GetByIdAsync(any int) to return null, simulating Product Not Found
            _mockProductRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Product?)null);
            // Create a DTO requesting ProductId=999 (our mock will indeed say NULL)
            var orderDto = new OrderCreateDTO
            {
                CustomerId = customerId,
                Items = new List<OrderItemDTO> { new() { ProductId = 999, Quantity = 1 } }
            };

            // Act & Assert: expecting NotFoundException because the service calls GetByIdAsync(999) → null
            await Assert.ThrowsAsync<NotFoundException>(() =>
                _orderService.CreateOrderAsync(orderDto)
            );
        }

        // Unit Test for Insufficient Stock
        [Fact]
        public async Task CreateOrderAsync_InsufficientStock_ThrowsInvalidOperationException()
        {
            // Define a valid customerId and return a real Customer from the mock
            int customerId = 1;
            _mockCustomerRepo
            .Setup(r => r.GetByIdAsync(customerId))
            .ReturnsAsync(new Customer { Id = customerId });
            // Setup ProductRepository.GetByIdAsync(10) to return a Product with only Stock=1
            _mockProductRepo
            .Setup(r => r.GetByIdAsync(10))
            .ReturnsAsync(new Product { Id = 10, Stock = 1 });
            // Build a DTO requesting Quantity=5 of ProductId=10 (only 1 in stock)
            var orderDto = new OrderCreateDTO
            {
                CustomerId = customerId,
                Items = new List<OrderItemDTO> { new() { ProductId = 10, Quantity = 5 } }
            };

            // Act & Assert: the service should throw InvalidOperationException because 5 > stock(1)
            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                _orderService.CreateOrderAsync(orderDto)
            );
        }

        // Unit Test for Fetching an Existing Order 
        [Fact]
        public async Task GetOrderByIdAsync_ExistingOrder_ReturnsOrderResponse()
        {
            // We define an orderId and construct an Order entity (as if it were already in the DB)
            int orderId = 100;
            var order = new Order
            {
                Id = orderId,
                CustomerId = 1,
                BaseAmount = 150,
                DiscountAmount = 0,
                TotalAmount = 150,
                OrderItems = new List<OrderItem>
                {
                    new OrderItem { ProductId = 10, Quantity = 3, UnitPrice = 50, LineTotal = 150 }
                }
            };
            // Setup the order repository mock to return that Order whenever GetByIdAsync(orderId) is called
            _mockOrderRepo
            .Setup(r => r.GetByIdAsync(orderId))
            .ReturnsAsync(order);

            // Act: call the service’s GetOrderByIdAsync(orderId)
            var result = await _orderService.GetOrderByIdAsync(orderId);

            // Assert: the returned OrderResponseDTO should not be null
            Assert.NotNull(result);
            // Assert: the returned DTO’s OrderId should match the one we passed in
            Assert.Equal(orderId, result.OrderId);
            // Assert: the returned DTO’s CustomerId should be 1
            Assert.Equal(1, result.CustomerId);
            // Assert: Because the Order had exactly one OrderItem, the DTO should have one item
            Assert.Single(result.OrderItems);
        }

        //Unit Test for Fetching a Non-Existent Order 
        [Fact]
        public async Task GetOrderByIdAsync_OrderMissing_ReturnsNull()
        {
            // Arrange: pick an orderId that does not exist (e.g., 999).
            int missingOrderId = 999;
            // Configure the IOrderRepository mock to return null whenever GetByIdAsync is called
            //    with any integer (including 999). This simulates Order Not Found in the database.
            _mockOrderRepo
            .Setup(r => r.GetByIdAsync(It.IsAny<int>()))
            .ReturnsAsync((Order?)null);

            // Act: call the service’s GetOrderByIdAsync method with the missingOrderId.
            //    Because of our setup, the mock will return null, and the service should propagate that.
            var result = await _orderService.GetOrderByIdAsync(missingOrderId);

            // Assert: since no order exists, the service should return null rather than throwing.
            Assert.Null(result);
            // (Optional) Verify that the repository’s GetByIdAsync was indeed called exactly once
            //    with the missingOrderId. This ensures the service tried to fetch the order.
            _mockOrderRepo.Verify(r => r.GetByIdAsync(missingOrderId), Times.Once);
        }
    }
}