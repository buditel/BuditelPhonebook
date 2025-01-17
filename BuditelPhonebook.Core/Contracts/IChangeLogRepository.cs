using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;

namespace BuditelPhonebook.Core.Contracts
{
    public interface IChangeLogRepository
    {
        Task AddChangeAsync(ChangeLog change);

        Task<List<string>> GenerateChangeDescription(Person oldPerson, EditPersonViewModel newPerson);

        IQueryable<ChangeLog> GetAllAttached();

    }
}
