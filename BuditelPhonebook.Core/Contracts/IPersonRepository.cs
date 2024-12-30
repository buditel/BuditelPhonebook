using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllAsync();
        Task<Person> GetByIdAsync(int id);
        Task<Person> GetByIdWithRelationsAsync(int id);

        Task AddAsync(Person person);
        Task UpdateAsync(Person person);
        Task DeleteAsync(int id);
        Task<IEnumerable<Person>> SearchAsync(string query);
        IEnumerable<Role> GetRoles();
        IEnumerable<Department> GetDepartments();

        Task<IEnumerable<SuggestionsViewModel>> GetSearchSuggestionsAsync(string query);
    }

}
