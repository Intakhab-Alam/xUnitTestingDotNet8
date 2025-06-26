using ProductCatalog.API.Data;
using ProductCatalog.API.Models;
namespace ProductCatalog.API.Repositories
{
    public class CustomerRepository : ICustomerRepository
    {
        private readonly ApplicationDbContext _dbContext;
        public CustomerRepository(ApplicationDbContext dbContext) { _dbContext = dbContext; }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _dbContext.Customers.FindAsync(id);
        }
    }
}