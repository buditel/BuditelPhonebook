using BuditelPhonebook.Infrastructure.Data.Models;
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
        Task SoftDeleteAsync(int id, string? comment, string leaveDate);

        Task<(IEnumerable<PersonDetailsViewModel> People, int TotalCount)> SearchAsync(string query, int page, int pageSize);
        IEnumerable<Role> GetRoles();
        IEnumerable<Department> GetDepartments();
        Task<Person> CreateANewPerson(CreatePersonViewModel model);
        Task<EditPersonViewModel> MapPersonForEditById(int id);

        Task EditPerson(EditPersonViewModel model);
    }

}
