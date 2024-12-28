using BuditelPhonebook.Data;
using BuditelPhonebook.Models;
using BuditelPhonebook.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .AsNoTracking() // Improves performance for read-only queries
                .ToListAsync();
        }

        public async Task<Person> GetByIdAsync(int id)
        {
            return await _context.People.FindAsync(id);
        }

        public async Task<Person> GetByIdWithRelationsAsync(int id)
        {
            return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Person person)
        {
            _context.People.Add(person);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Person person)
        {
            _context.People.Update(person);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                // Optional: Log a warning that the person was not found
                return;
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<Person>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await _context.People
                    .Include(p => p.Role) // Include Role navigation property
                    .Include(p => p.Department) // Include Department navigation property
                    .ToListAsync();

            query = query.ToLower();
            return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .Where(p => p.FirstName.ToLower().Contains(query)
                            || (p.MiddleName != null && p.MiddleName.ToLower().Contains(query))
                            || p.LastName.ToLower().Contains(query)
                            || p.Email.ToLower().Contains(query)
                            || p.BusinessPhoneNumber.ToLower().Contains(query)
                            || (p.Role != null && p.Role.Name.ToLower().Contains(query)) // Ensure Role.Name is accessed
                            || (p.Department != null && p.Department.Name.ToLower().Contains(query))) // Ensure Department.Name is accessed
                .ToListAsync();
        }

        public IEnumerable<Role> GetRoles()
        {
            return _context.Roles.AsNoTracking().ToList();
        }

        public IEnumerable<Department> GetDepartments()
        {
            return _context.Departments.AsNoTracking().ToList();
        }

        public async Task<IEnumerable<SuggestionsViewModel>> GetSearchSuggestionsAsync(string query)
        {
            query = query.ToLower();

            var results = await _context.People
            .Include(p => p.Department)
            .Where(p => !p.IsDeleted &&
                        (p.FirstName.ToLower().Contains(query) ||
                         p.LastName.ToLower().Contains(query)) ||
                         (p.Subject != null && p.Subject.ToLower().Contains(query)) ||
                         p.Department.Name.ToLower().Contains(query))
            .Select(p => new SuggestionsViewModel
            {
                Id = p.Id,
                FullName = $"{p.FirstName} {p.LastName}",
                Subject = p.Subject,
                Department = p.Department.Name
            })
            .Take(10)
            .ToListAsync();

            return results;
        }
    }
}
