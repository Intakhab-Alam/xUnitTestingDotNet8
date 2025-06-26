using ProductCatalog.API.Models;
namespace ProductCatalog.API.Repositories
{
    public interface ICustomerRepository
    {
        Task<Customer?> GetByIdAsync(int id);
    }
}