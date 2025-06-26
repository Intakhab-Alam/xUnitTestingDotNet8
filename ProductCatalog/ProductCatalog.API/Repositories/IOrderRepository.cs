using ProductCatalog.API.Models;
namespace ProductCatalog.API.Repositories
{
    public interface IOrderRepository
    {
        Task<Order?> GetByIdAsync(int id);
        Task AddAsync(Order order);
        Task SaveChangesAsync();
    }
}