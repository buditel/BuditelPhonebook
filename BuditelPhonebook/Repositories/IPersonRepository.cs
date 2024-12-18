using BuditelPhonebook.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace BuditelPhonebook.Repositories
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllAsync();
        Task<Person> GetByIdAsync(int id);
        Task AddAsync(Person person);
        Task UpdateAsync(Person person);
        Task DeleteAsync(int id);
        Task<IEnumerable<Person>> SearchAsync(string query);
    }

}
