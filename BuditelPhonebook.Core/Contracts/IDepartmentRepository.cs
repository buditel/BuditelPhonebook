using BuditelPhonebook.Infrastructure.Data.Models;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IDepartmentRepository
    {
        Task<IEnumerable<Department>> GetAllAsync();
        IQueryable<Department> GetAllAttached();

        Task<Department> GetByIdAsync(int id);
        Task AddAsync(Department department);
        Task UpdateAsync(Department department);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);

    }

}
