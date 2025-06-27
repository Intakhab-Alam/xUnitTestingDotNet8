using Moq;
using Microsoft.AspNetCore.Mvc;
using ProductCatalog.API.Services;
using ProductCatalog.API.DTOs;
using ProductCatalog.API.Controllers;

namespace ProductCatalog.Tests.Controllers
{
    public class OrdersControllerTests
    {
        // Mock object for IOrderService interface to fake service behaviour during tests
        private readonly Mock<IOrderService> _mockOrderService;

        // Instance of the controller being tested, injected with the mock service
        private readonly OrdersController _controller;

        // Constructor runs before each test method, initializes mock order service and controller
        public OrdersControllerTests()
        {
            // Create a new Mock<IOrderService> instance
            _mockOrderService = new Mock<IOrderService>();

            // Instantiate OrdersController passing in the mocked service object
            // The .Object property returns the actual IOrderService proxy from the mock
            _controller = new OrdersController(_mockOrderService.Object);
        }

        // Test method: GET /order/{id} with an existing order ID returns 200 OK and the order data
        [Fact]
        public async Task GetOrder_ExistingId_ReturnsOkWithOrder()
        {
            // Arrange: setup test data and mock behavior
            var orderId = 1;  // The order ID to test
            var orderResponse = new OrderResponseDTO { OrderId = orderId, CustomerId = 123 };  // Mock order data to be returned

            // Setup the mock service to return orderResponse when GetOrderByIdAsync is called with orderId
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(orderId))
                             .ReturnsAsync(orderResponse);

            // Act: invoke the GetOrder controller action with the orderId
            var result = await _controller.GetOrder(orderId);

            // Assert: verify the result is OkObjectResult containing the expected orderResponse
            var okResult = Assert.IsType<OkObjectResult>(result);  // Checks that result is 200 OK with an object
            Assert.Equal(orderResponse, okResult.Value);  // Checks that the returned object matches the mocked data
        }

        // Test method: GET /order/{id} with a non-existing order ID returns 404 NotFound
        [Fact]
        public async Task GetOrder_NonExistingId_ReturnsNotFound()
        {
            // Arrange: setup the mock to return null for any integer argument (simulate not found)
            _mockOrderService.Setup(s => s.GetOrderByIdAsync(It.IsAny<int>()))
                             .ReturnsAsync((OrderResponseDTO?)null);

            // Act: call GetOrder with a non-existing ID (999)
            var result = await _controller.GetOrder(999);

            // Assert: verify that the result is NotFoundResult (HTTP 404)
            Assert.IsType<NotFoundResult>(result);
        }

        // Test method: POST /order with valid model returns 201 Created with location header
        [Fact]
        public async Task CreateOrder_ValidModel_ReturnsOkResult()
        {
            // Arrange: create input DTO for creating an order
            var orderDto = new OrderCreateDTO { CustomerId = 1 };

            // Expected response DTO after successful creation
            var createdOrder = new OrderResponseDTO { OrderId = 1, CustomerId = 1 };

            // Setup mock to return createdOrder when CreateOrderAsync is called with orderDto
            _mockOrderService.Setup(s => s.CreateOrderAsync(orderDto))
                             .ReturnsAsync(createdOrder);

            // Act: call CreateOrder action with the valid DTO
            var result = await _controller.CreateOrder(orderDto);

            // Assert: verify the result is OkObjectResult (HTTP 200)
            var okResult = Assert.IsType<OkObjectResult>(result);

            // Verify that the returned object is the expected created order DTO
            Assert.Equal(createdOrder, okResult.Value);
        }

        // Test method: POST /order with invalid model returns 400 BadRequest
        [Fact]
        public async Task CreateOrder_InvalidModel_ReturnsBadRequest()
        {
            // Arrange: manually add a model validation error to simulate invalid input
            _controller.ModelState.AddModelError("CustomerId", "Required");

            // Act: call CreateOrder with an empty DTO (which is invalid due to missing CustomerId)
            var result = await _controller.CreateOrder(new OrderCreateDTO());

            // Assert: verify the response is BadRequestObjectResult (HTTP 400)
            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);

            // Verify that BadRequest contains some error details (not null)
            Assert.NotNull(badRequestResult.Value);
        }
    }
}