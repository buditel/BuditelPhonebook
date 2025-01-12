using BuditelPhonebook.Infrastructure.Data.Models;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IUserRoleRepository
    {
        Task<IEnumerable<UserRole>> GetAllRolesAsync();
        Task<UserRole> GetByIdAsync(int id);

        Task AddRoleAsync(UserRole userRole);
        Task RemoveRoleAsync(int id);
        Task<bool> RoleExistsAsync(UserRole userRole);

        Task<IEnumerable<string>> GetAdminEmailsAsync();
        Task<IEnumerable<string>> GetModeratorEmailsAsync();
    }
}
