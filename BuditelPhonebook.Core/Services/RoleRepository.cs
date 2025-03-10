﻿using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Core.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            return await _context.Roles.ToListAsync();
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            return await _context.Roles.FindAsync(id);
        }

        public async Task AddAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _context.Roles.Add(role);
            await _context.SaveChangesAsync();
        }


        public async Task UpdateAsync(Role role)
        {
            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }


        public async Task DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                throw new ArgumentNullException(nameof(role));
            }

            role.IsDeleted = true;

            _context.Roles.Update(role);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Role> GetAllAttached()
        {
            return _context.Roles.AsQueryable();
        }
    }

}
