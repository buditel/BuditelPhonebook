using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Core.Services
{
    public class UserRoleRepository : IUserRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public UserRoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<UserRole>> GetAllRolesAsync()
        {
            return await _context.UsersRoles.AsNoTracking().ToListAsync();
        }

        public async Task<UserRole> GetByIdAsync(int id)
        {
            return await _context.UsersRoles.FindAsync(id);
        }

        public async Task AddRoleAsync(UserRole userRole)
        {
            if (!await RoleExistsAsync(userRole))
            {
                await _context.UsersRoles.AddAsync(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveRoleAsync(int id)
        {
            var userRole = await _context.UsersRoles
                .FindAsync(id);
            if (userRole != null)
            {
                _context.UsersRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> RoleExistsAsync(UserRole userRole)
        {
            return await _context.UsersRoles.AnyAsync(ur => ur.Email == userRole.Email && ur.Role == userRole.Role);
        }

        public async Task<IEnumerable<string>> GetAdminEmailsAsync()
        {
            return await _context.UsersRoles
                .Where(ur => ur.Role == "Admin")
                .Select(ur => ur.Email)
                .ToListAsync();
        }

        public async Task<IEnumerable<string>> GetModeratorEmailsAsync()
        {
            return await _context.UsersRoles
                .Where(ur => ur.Role == "Moderator")
                .Select(ur => ur.Email)
                .ToListAsync();
        }

        public IQueryable<UserRole> GetAllRolesAttached()
        {
            return _context.UsersRoles.AsQueryable();
        }

        public async Task UpdateAsync(UserRole userRole)
        {
            if (userRole == null)
            {
                throw new ArgumentNullException(nameof(userRole));
            }

            _context.UsersRoles.Update(userRole);
            await _context.SaveChangesAsync();
        }
    }
}
