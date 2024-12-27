using BuditelPhonebook.Contracts;
using BuditelPhonebook.Repositories;
using BuditelPhonebook.ViewModels;
using Microsoft.AspNetCore.Mvc;

namespace BuditelPhonebook.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SuggestionsController : ControllerBase
    {
        private readonly IPersonRepository _personRepository;

        public SuggestionsController(IPersonRepository personRepository)
        {
            _personRepository = personRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetSuggestions([FromQuery] string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return Ok(new List<string>());
            }

            IEnumerable<SuggestionsViewModel> results = await _personRepository.GetSearchSuggestionsAsync(query);

            return Ok(results);
        }
    }
}
