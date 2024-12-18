using BuditelPhonebook.Data;
using BuditelPhonebook.Models;
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
            return await _context.People.ToListAsync();
        }
        public async Task<Person> GetByIdAsync(int id)
        {
            return await _context.People.FindAsync(id);
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
            if (person != null)
            {
                _context.People.Remove(person);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<IEnumerable<Person>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
                return await GetAllAsync();

            query = query.ToLower();
            return await _context.People
                .Where(p => p.FirstName.ToLower().Contains(query)
                            || (p.MiddleName != null && p.MiddleName.ToLower().Contains(query))
                            || p.LastName.ToLower().Contains(query)
                            || p.Email.ToLower().Contains(query)
                            || p.PhoneNumber.ToLower().Contains(query)
                            || p.Role.ToLower().Contains(query)
                            || p.Department.ToLower().Contains(query))
                .ToListAsync();
        }
    }
}
