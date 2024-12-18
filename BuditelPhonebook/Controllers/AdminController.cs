// Updated AdminController.cs with logger and exception handling
using BuditelPhonebook.Models;
using BuditelPhonebook.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System;
using System.Threading.Tasks;

namespace BuditelPhonebook.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly IPersonRepository _personRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IPersonRepository personRepository, ILogger<AdminController> logger)
        {
            _personRepository = personRepository;
            _logger = logger;
        }

        public async Task<IActionResult> Index()
        {
            try
            {
                var people = await _personRepository.GetAllAsync();
                return View(people);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching people in AdminController.Index");
                return StatusCode(500, "Internal server error");
            }
        }

        public IActionResult Create()
        {
            var newPerson = new Person(); // Initialize a new Person for the form
            return View(newPerson);
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Person person)
        {
            if (!ModelState.IsValid)
                return View(person);

            try
            {
                await _personRepository.AddAsync(person);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating person in AdminController.Create");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the person.");
                return View(person);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(id);
                if (person == null) return NotFound();
                return View(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching person with ID {id} in AdminController.Edit");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, Person person)
        {
            if (id != person.Id) return NotFound();

            if (!ModelState.IsValid)
                return View(person);

            try
            {
                await _personRepository.UpdateAsync(person);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating person with ID {id} in AdminController.Edit");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the person.");
                return View(person);
            }
        }

        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(id);
                if (person == null) return NotFound();
                return View(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching person with ID {id} in AdminController.Delete");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _personRepository.DeleteAsync(id);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting person with ID {id} in AdminController.DeleteConfirmed");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
