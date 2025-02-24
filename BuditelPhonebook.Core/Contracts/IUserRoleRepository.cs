using BuditelPhonebook.Infrastructure.Data.Models;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetAllRolesAsync();
        IQueryable<UserRole> GetAllRolesAttached();
        Task<UserRole> GetByIdAsync(int id);

        Task UpdateAsync(UserRole userRole);
        Task AddRoleAsync(UserRole userRole);
        Task RemoveRoleAsync(int id);
        Task<bool> RoleExistsAsync(UserRole userRole);

        Task<IEnumerable<string>> GetAdminEmailsAsync();
        Task<IEnumerable<string>> GetModeratorEmailsAsync();
    }
}
