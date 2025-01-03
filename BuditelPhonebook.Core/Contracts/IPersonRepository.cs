using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels;
using BuditelPhonebook.Web.ViewModels.Person;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IPersonRepository
    {
        Task<IEnumerable<Person>> GetAllAsync();
        IQueryable<Person> GetAllAttached();

        Task<Person> GetByIdAsync(int id);
        Task<Person> GetByIdWithRelationsAsync(int id);

        Task AddAsync(Person person);
        Task UpdateAsync(Person person);
        Task DeleteAsync(int id);
        Task SoftDeleteAsync(int id, string? comment);

        Task<IEnumerable<PersonDetailsViewModel>> SearchAsync(string query);
        IEnumerable<Role> GetRoles();
        IEnumerable<Department> GetDepartments();

        Task<IEnumerable<SuggestionsViewModel>> GetSearchSuggestionsAsync(string query);

        Task<Person> CreateANewPerson(CreatePersonViewModel model);
        Task<EditPersonViewModel> MapPersonForEditById(int id);

        Task EditPerson(EditPersonViewModel model);
    }

}
