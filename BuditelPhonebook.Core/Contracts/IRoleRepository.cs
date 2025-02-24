using BuditelPhonebook.Infrastructure.Data.Models;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IRoleRepository
    {
        Task<IEnumerable<Role>> GetAllAsync();
        IQueryable<Role> GetAllAttached();
        Task<Role> GetByIdAsync(int id);
        Task AddAsync(Role role);
        Task UpdateAsync(Role role);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
    }

}
