using BuditelPhonebook.Core.Contracts;
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
        public async Task<IActionResult> Index(string search)
        {
            var people = await _personRepository.SearchAsync(search); // Filter results based on the search query
            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_PeopleTablePartial", people); // Return the partial view for AJAX
            }
            return View(people); // Return the full view for standard requests
        }


        public async Task<IActionResult> Details(int id)
        {
            return View();
        }

    }
}
