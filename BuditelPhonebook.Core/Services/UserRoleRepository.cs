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
            try
            {
                return await _context.UsersRoles.AsNoTracking().ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички потребителски роли.", ex);
            }
        }

        public async Task<UserRole> GetByIdAsync(int id)
        {
            var userRole = await _context.UsersRoles.FindAsync(id);

            if (userRole == null)
            {
                throw new KeyNotFoundException($"Потребителска роля с ID {id} не е намерена.");
            }

            return userRole;
        }

        public async Task AddRoleAsync(UserRole userRole)
        {
            if (!await RoleExistsAsync(userRole))
            {
                try
                {
                    await _context.UsersRoles.AddAsync(userRole);
                    await _context.SaveChangesAsync();
                }
                catch (Exception ex)
                {
                    throw new ApplicationException("Грешка при добваяне на нова потребителска роля.", ex);
                }
            }
        }

        public async Task RemoveRoleAsync(int id)
        {
            var userRole = await _context.UsersRoles.FindAsync(id);

            if (userRole == null)
            {
                throw new KeyNotFoundException($"Грешка при изтриване на потребителска роля от потребител с ID {id}.");
            }

            try
            {
                _context.UsersRoles.Remove(userRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при изтриване на потребителска роля.", ex);
            }

        }

        public async Task<bool> RoleExistsAsync(UserRole userRole)
        {
            try
            {
                return await _context.UsersRoles.AnyAsync(ur => ur.Email == userRole.Email && ur.Role == userRole.Role);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при търсене на роля.", ex);
            }
        }

        public async Task<IEnumerable<string>> GetAdminEmailsAsync()
        {
            try
            {
                return await _context.UsersRoles
                .Where(ur => ur.Role == "Admin")
                .Select(ur => ur.Email)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на админски имейли.", ex);
            }
        }

        public async Task<IEnumerable<string>> GetModeratorEmailsAsync()
        {
            try
            {
                return await _context.UsersRoles
                .Where(ur => ur.Role == "Moderator")
                .Select(ur => ur.Email)
                .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на модераторски имейли.", ex);
            }
        }

        public IQueryable<UserRole> GetAllRolesAttached()
        {
            try
            {
                return _context.UsersRoles.AsQueryable();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички потребителски роли.", ex);
            }
        }

        public async Task UpdateAsync(UserRole userRole)
        {
            try
            {
                _context.UsersRoles.Update(userRole);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при актуализиране на потребителска роля с ID {userRole.Id}.", ex);
            }
        }
    }
}
