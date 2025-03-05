using BuditelPhonebook.Core.Contracts;
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
            try
            {
                return await _context.Departments.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички отдели.", ex);
            }
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                throw new KeyNotFoundException($"Отдел с ID {id} не е намерен.");
            }

            return department;
        }

        public async Task AddAsync(Department department)
        {
            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при добавяне на нов отдел.", ex);
            }
        }

        public async Task UpdateAsync(Department department)
        {
            try
            {
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при актуализиране на отдел с ID {department.Id}.", ex);
            }
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
                throw new KeyNotFoundException($"Отдел с ID {id} не е намерен.");
            }

            department.IsDeleted = true;

            try
            {
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при софт-изтриване на отдел с ID {id}.", ex);
            }
        }

        public IQueryable<Department> GetAllAttached()
        {
            try
            {
                return _context.Departments.AsQueryable();
            }
            catch (ApplicationException ex)
            {

                throw new ApplicationException("Грешка при извличане на отделите.", ex);
            }
        }
    }
}
