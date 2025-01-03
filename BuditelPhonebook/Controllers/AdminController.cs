// Updated AdminController.cs with logger and exception handling
using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Web.Controllers
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

        [HttpGet]
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

        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreatePersonViewModel();

            model.Roles = _personRepository.GetRoles(); // Add a method in IPersonRepository
            model.Departments = _personRepository.GetDepartments(); // Add a method in IPersonRepository

            // Pass a new Person instance to the view
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePersonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _personRepository.GetRoles();
                model.Departments = _personRepository.GetDepartments();
                return View(model);
            }

            Person person = await _personRepository.CreateANewPerson(model);

            try
            {
                await _personRepository.AddAsync(person);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating person in AdminController.Create");
                ModelState.AddModelError(string.Empty, "An error occurred while saving the person.");
                model.Roles = _personRepository.GetRoles();
                model.Departments = _personRepository.GetDepartments();
                return View(model);
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                // Fetch the Person using the repository
                var model = await _personRepository.MapPersonForEditById(id);

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching person with ID {id} in AdminController.Edit");
                return StatusCode(500, "Internal server error");
            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPersonViewModel model)
        {
            if (!ModelState.IsValid)
            {
                model.Roles = _personRepository.GetRoles();
                model.Departments = _personRepository.GetDepartments();
                return View(model);
            }

            try
            {
                await _personRepository.EditPerson(model);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating person with ID {model.Id} in AdminController.Edit");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the person.");
                model.Roles = _personRepository.GetRoles();
                model.Departments = _personRepository.GetDepartments();
                return View(model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(id);
                if (person == null)
                {
                    return NotFound();
                }

                var model = new DeletePersonViewModel
                {
                    Id = id,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName
                };

                return View(model);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error fetching person with ID {id} in AdminController.Delete");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DeletePersonViewModel model)
        {
            try
            {
                await _personRepository.SoftDeleteAsync(model.Id, model.CommentOnDeletion);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting person with ID {model.Id} in AdminController.DeleteConfirmed");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpGet]
        public async Task<IActionResult> DeletedIndex()
        {
            var models = await _personRepository.GetAllAttached()
                .Where(p => p.IsDeleted)
                .Select(p => new DeletedIndexPersonViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    Email = p.Email,
                    PersonalPhoneNumber = p.PersonalPhoneNumber,
                    Department = p.Department.Name,
                    Role = p.Role.Name,
                    CommentOnDeletion = p.CommentOnDeletion
                })
                .ToListAsync();

            return View(models);
        }

        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            var person = await _personRepository.GetByIdAsync(id);

            var model = new RestorePersonViewModel
            {
                Id = id,
                FirstName = person.FirstName,
                MiddleName = person.MiddleName,
                LastName = person.LastName
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> Restore(RestorePersonViewModel model)
        {
            var person = await _personRepository.GetByIdAsync(model.Id);

            person.CommentOnDeletion = null;
            person.IsDeleted = false;

            await _personRepository.UpdateAsync(person);

            return RedirectToAction(nameof(DeletedIndex));
        }
    }
}
