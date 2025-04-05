using BuditelPhonebook.Infrastructure.Data.Models;

namespace BuditelPhonebook.Core.Contracts
{
    public interface ISubjectRepository
    {
        Task<IEnumerable<Subject>> GetAllAsync();
        IQueryable<Subject> GetAllAttached();
        Task<Subject> GetByIdAsync(int id);
        Task AddAsync(Subject subject);
        Task UpdateAsync(Subject subject);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id);
    }
}
