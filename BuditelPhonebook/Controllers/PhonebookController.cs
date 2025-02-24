using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BuditelPhonebook.Web.Controllers
{
    [Authorize]
    public class PhonebookController : Controller
    {
        private readonly IPersonRepository _personRepository;

        public PhonebookController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [HttpGet]
        public async Task<IActionResult> Index(string search, int page = 1, int pageSize = 10)
        {
            var (people, totalCount) = await _personRepository.SearchAsync(search, page, pageSize);

            var model = new PaginatedPersonViewModel
            {
                People = people,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PeopleTablePartial", model); // Return the partial view for AJAX
            }

            return View(model); // Return the full view for standard requests
        }

        [HttpGet]
        public async Task<IActionResult> Details(int id)
        {
            Person person = await _personRepository.GetByIdWithRelationsAsync(id);

            if (person == null)
            {
                return NotFound();
            }

            var model = new PersonDetailsViewModel
            {
                Id = id,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName,
                BusinessPhoneNumber = person.BusinessPhoneNumber,
                PersonalPhoneNumber = person.PersonalPhoneNumber,
                Birthdate = person.Birthdate,
                Role = person.Role.Name,
                Department = person.Department.Name,
                Email = person.Email,
                SubjectGroup = person.SubjectGroup,
                Subject = person.Subject,
                PersonPicture = person.PersonPicture
            };

            return View(model);
        }

    }
}
