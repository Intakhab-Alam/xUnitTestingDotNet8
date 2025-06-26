using ProductCatalog.API.DTOs;
using ProductCatalog.API.Exceptions;
using ProductCatalog.API.Models;
using ProductCatalog.API.Repositories;
namespace ProductCatalog.API.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IProductRepository _productRepository;
        private readonly ICustomerRepository _customerRepository;

        public OrderService(
            IOrderRepository orderRepository,
            IProductRepository productRepository,
            ICustomerRepository customerRepository)
        {
            _orderRepository = orderRepository;
            _productRepository = productRepository;
            _customerRepository = customerRepository;
        }

        public async Task<OrderResponseDTO?> CreateOrderAsync(OrderCreateDTO orderCreateDto)
        {
            var customer = await _customerRepository.GetByIdAsync(orderCreateDto.CustomerId);
            if (customer == null)
                throw new NotFoundException($"Customer with id {orderCreateDto.CustomerId} not found.");

            if (orderCreateDto.Items == null || !orderCreateDto.Items.Any())
                throw new ArgumentException("Order must have at least one item.");

            try
            {
                var order = new Order
                {
                    CustomerId = customer.Id,
                    OrderDate = DateTime.UtcNow,
                    OrderItems = new List<OrderItem>()
                };

                decimal baseAmount = 0;

                foreach (var itemDto in orderCreateDto.Items)
                {
                    var product = await _productRepository.GetByIdAsync(itemDto.ProductId);
                    if (product == null)
                        throw new NotFoundException($"Product with id {itemDto.ProductId} not found.");

                    if (itemDto.Quantity <= 0)
                        throw new ArgumentException("Quantity must be greater than zero.");

                    if (product.Stock < itemDto.Quantity)
                        throw new InvalidOperationException($"Not enough stock for product {product.Name}. Available: {product.Stock}, requested: {itemDto.Quantity}");

                    decimal lineTotal = product.Price * itemDto.Quantity;

                    order.OrderItems.Add(new OrderItem
                    {
                        ProductId = product.Id,
                        Quantity = itemDto.Quantity,
                        UnitPrice = product.Price,
                        LineTotal = lineTotal
                    });

                    baseAmount += lineTotal;

                    // Deduct stock
                    product.Stock -= itemDto.Quantity;
                }

                order.BaseAmount = baseAmount;

                // Calculate discount at order level
                order.DiscountAmount = CalculateDiscount(baseAmount);

                order.TotalAmount = baseAmount - order.DiscountAmount;

                await _orderRepository.AddAsync(order);
                await _productRepository.SaveChangesAsync();
                await _orderRepository.SaveChangesAsync();

                return MapToOrderDto(order, customer);
            }
            catch
            {
                throw; // Let upper layer handle exceptions
            }
        }

        // Dummy discount logic: 5% discount for orders above 5000
        private decimal CalculateDiscount(decimal baseAmount)
        {
            if (baseAmount > 5000)
                return baseAmount * 0.05m;
            return 0;
        }

        public async Task<OrderResponseDTO?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null) return null;

            var customer = order.Customer ?? await _customerRepository.GetByIdAsync(order.CustomerId);

            return MapToOrderDto(order, customer);
        }

        private OrderResponseDTO MapToOrderDto(Order order, Customer? customer)
        {
            return new OrderResponseDTO
            {
                OrderId = order.Id,
                CustomerId = order.CustomerId,
                CustomerName = customer?.Name ?? string.Empty,
                CustomerEmail = customer?.Email ?? string.Empty,
                OrderDate = order.OrderDate,
                BaseAmount = order.BaseAmount,
                Discount = order.DiscountAmount,
                TotalAmount = order.TotalAmount,
                OrderItems = order.OrderItems.Select(item => new OrderItemResponseDTO
                {
                    OrderItemId = item.Id,
                    ProductId = item.ProductId,
                    ProductName = item.Product?.Name ?? string.Empty,
                    Description = item.Product?.Description ?? string.Empty,
                    Quantity = item.Quantity,
                    UnitPrice = item.UnitPrice,
                    LineTotal = item.LineTotal
                }).ToList()
            };
        }
    }
}