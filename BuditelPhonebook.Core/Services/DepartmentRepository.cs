﻿using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Core.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            return await _context.Departments.ToListAsync();
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            return await _context.Departments.FindAsync(id);
        }

        public async Task AddAsync(Department department)
        {
            _context.Departments.Add(department);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Department department)
        {
            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                throw new ArgumentNullException(nameof(department));
            }

            department.IsDeleted = true;

            _context.Departments.Update(department);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Department> GetAllAttached()
        {
            return _context.Departments.AsQueryable();
        }
    }
}
