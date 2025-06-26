using ProductCatalog.API.DTOs;
namespace ProductCatalog.API.Services
{
    public interface IOrderService
    {
        Task<OrderResponseDTO?> CreateOrderAsync(OrderCreateDTO orderCreateDto);
        Task<OrderResponseDTO?> GetOrderByIdAsync(int id);
    }
}