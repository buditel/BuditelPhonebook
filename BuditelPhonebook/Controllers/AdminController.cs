// Updated AdminController.cs with logger and exception handling
using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;
using BuditelPhonebook.Web.ViewModels.UserRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BuditelPhonebook.Common.EntityValidationConstants.Person;
using static BuditelPhonebook.Common.EntityValidationMessages.Person;

namespace BuditelPhonebook.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IPersonRepository personRepository, IUserRoleRepository userRoleRepository, ILogger<AdminController> logger)
        {
            _personRepository = personRepository;
            _userRoleRepository = userRoleRepository;
            _logger = logger;
        }

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
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

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet]
        public IActionResult Create()
        {
            var model = new CreatePersonViewModel();

            model.Roles = _personRepository.GetRoles(); // Add a method in IPersonRepository
            model.Departments = _personRepository.GetDepartments(); // Add a method in IPersonRepository

            // Pass a new Person instance to the view
            return View(model);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePersonViewModel model)
        {
            var exists = _personRepository.GetAllAttached().Any(r => r.Email == model.Email);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Email), EmailUniqueMessage);
            }

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

        [Authorize(Roles = "SuperAdmin, Admin")]
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

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPersonViewModel model)
        {
            bool exists = _personRepository.GetAllAttached().Any(r => r.Email == model.Email);
            var currentPerson = await _personRepository.GetByIdAsync(model.Id);
            if (exists && model.Email != currentPerson.Email)
            {
                ModelState.AddModelError(nameof(model.Email), EmailUniqueMessage);
            }

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

        [Authorize(Roles = "SuperAdmin, Admin")]
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

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DeletePersonViewModel model)
        {
            try
            {
                await _personRepository.SoftDeleteAsync(model.Id, model.CommentOnDeletion, model.LeaveDate);

                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting person with ID {model.Id} in AdminController.DeleteConfirmed");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet]
        public async Task<IActionResult> DeletedIndex()
        {
            var models = await _personRepository.GetAllAttached()
                .Where(p => p.IsDeleted)
                .ToListAsync();

            var results = models
                .Select(p => new DeletedIndexPersonViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    Email = p.Email,
                    PersonalPhoneNumber = p.PersonalPhoneNumber,
                    LeaveDate = p.LeaveDate?.ToString(HireAndLeaveDateFormat),
                    Department = p.Department.Name,
                    Role = p.Role.Name,
                    CommentOnDeletion = p.CommentOnDeletion
                });

            return View(results);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
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

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        public async Task<IActionResult> Restore(RestorePersonViewModel model)
        {
            var person = await _personRepository.GetByIdAsync(model.Id);

            person.CommentOnDeletion = null;
            person.LeaveDate = null;
            person.IsDeleted = false;

            await _personRepository.UpdateAsync(person);

            return RedirectToAction(nameof(DeletedIndex));
        }

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
        [HttpGet]
        public async Task<IActionResult> UserRoles()
        {
            var userRoles = await _userRoleRepository.GetAllRolesAsync();

            var model = new UserRoleViewModel
            {
                UserRoles = userRoles
            };

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public IActionResult ConfirmUserRole(UserRoleViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View("UserRoles", model);
            }

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> AssignUserRole(UserRoleViewModel model)
        {
            if (model.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || model.Role.Equals("Moderator", StringComparison.OrdinalIgnoreCase))
            {
                var userRole = new UserRole()
                {
                    Role = model.Role,
                    Email = model.Email,
                };

                await _userRoleRepository.AddRoleAsync(userRole);
            }
            //TODO: ExceptionHandling

            return RedirectToAction("UserRoles");
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> ConfirmRemoveFromRole(int id)
        {
            var userRole = await _userRoleRepository.GetByIdAsync(id);

            if (userRole == null)
            {
                throw new Exception();
            }

            var model = new RemoveUserRoleViewModel()
            {
                Id = id,
                Email = userRole.Email,
                Role = userRole.Role
            };

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> RemoveFromRole(int id)
        {
            await _userRoleRepository.RemoveRoleAsync(id);

            return RedirectToAction("UserRoles");
        }
    }
}
