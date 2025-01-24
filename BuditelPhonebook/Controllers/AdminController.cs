// Updated AdminController.cs with logger and exception handling
using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.ChangeLog;
using BuditelPhonebook.Web.ViewModels.Person;
using BuditelPhonebook.Web.ViewModels.UserRole;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BuditelPhonebook.Common.EntityValidationConstants.ChangeLog;
using static BuditelPhonebook.Common.EntityValidationMessages.Person;
using static BuditelPhonebook.Common.EntityValidationMessages.UserRole;

namespace BuditelPhonebook.Web.Controllers
{
    public class AdminController : Controller
    {
        private readonly IPersonRepository _personRepository;
        private readonly IUserRoleRepository _userRoleRepository;
        private readonly IChangeLogRepository _changeLogRepository;
        private readonly IConfiguration _configuration;
        private readonly ILogger<AdminController> _logger;

        public AdminController(IPersonRepository personRepository,
            IUserRoleRepository userRoleRepository,
            IChangeLogRepository changeLogRepository,
            IConfiguration configuration,
            ILogger<AdminController> logger)
        {
            _personRepository = personRepository;
            _userRoleRepository = userRoleRepository;
            _changeLogRepository = changeLogRepository;
            _configuration = configuration;
            _logger = logger;
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

            try
            {
                Person person = await _personRepository.CreateANewPerson(model);

                await _personRepository.AddAsync(person);

                ChangeLog change = new ChangeLog()
                {
                    ChangedAt = DateTime.Now,
                    ChangedBy = User.Identity.Name,
                    ChangesDescriptions = new List<string> { "Създаден нов контакт." },
                    PersonId = person.Id,
                };

                await _changeLogRepository.AddChangeAsync(change);

                return RedirectToAction("Index", "Phonebook");
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
            var currentPerson = await _personRepository.GetByIdWithRelationsAsync(model.Id);
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
                var change = new ChangeLog
                {
                    PersonId = model.Id,
                    ChangedAt = DateTime.Now,
                    ChangedBy = User.Identity.Name,
                    ChangesDescriptions = await _changeLogRepository.GenerateChangeDescription(currentPerson, model)
                };

                await _personRepository.EditPerson(model);
                await _changeLogRepository.AddChangeAsync(change);

                return RedirectToAction("Index", "Phonebook");
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

                return RedirectToAction("DeletedIndex");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error deleting person with ID {model.Id} in AdminController.DeleteConfirmed");
                return StatusCode(500, "Internal server error");
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet]
        public async Task<IActionResult> DeletedIndex(string search, int page = 1, int pageSize = 10)
        {
            var (people, totalCount) = await _personRepository.SearchDeletedAsync(search, page, pageSize);

            var model = new PaginatedDeletedPersonViewModel
            {
                People = people,
                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount
            };

            if (Request.Headers["X-Requested-With"] == "XMLHttpRequest")
            {
                return PartialView("_DeletedPeoplePartial", model); // Return the partial view for AJAX
            }

            return View(model);
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
        public async Task<IActionResult> ConfirmUserRole(UserRoleViewModel model)
        {
            var superAdminEmails = _configuration.GetSection("SuperAdminEmails").Get<List<string>>();
            if (superAdminEmails != null && superAdminEmails.Contains(model.Email))
            {
                ModelState.AddModelError(nameof(model.Email), string.Format(UserInSuperAdminRoleMessage, model.Email));
            }

            if (await _userRoleRepository.GetAllRolesAttached().AnyAsync(ur => ur.Email == model.Email && ur.Role == model.Role))
            {
                var roleInBulgarian = model.Role == "Admin" ? "Администратор" : "Модератор";
                ModelState.AddModelError(nameof(model.Role), string.Format(UserIsInSameRoleMessage, model.Email, roleInBulgarian));
            }

            if (!ModelState.IsValid)
            {
                var userRoles = await _userRoleRepository.GetAllRolesAsync();
                model.UserRoles = userRoles;

                return View("UserRoles", model);
            }

            var currentRole = await _userRoleRepository.GetAllRolesAttached()
                .FirstOrDefaultAsync(ur => ur.Email == model.Email);

            if (currentRole != null)
            {
                model.CurrentRole = currentRole.Role;
            }

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> AssignUserRole(UserRoleViewModel model)
        {
            if (model.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase) || model.Role.Equals("Moderator", StringComparison.OrdinalIgnoreCase))
            {
                var userToChange = await _userRoleRepository.GetAllRolesAttached().FirstOrDefaultAsync(ur => ur.Email == model.Email);

                if (userToChange != null)
                {
                    userToChange.Role = model.Role;
                    await _userRoleRepository.UpdateAsync(userToChange);
                }
                else
                {
                    var userRole = new UserRole()
                    {
                        Role = model.Role,
                        Email = model.Email,
                    };

                    await _userRoleRepository.AddRoleAsync(userRole);
                }
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

        public async Task<IActionResult> SeeLatestChange(int id)
        {
            var model = await _changeLogRepository
                .GetAllAttached()
                .Where(cl => cl.PersonId == id)
                .OrderByDescending(cl => cl.ChangedAt)
                .Select(cl => new ChangeLogViewModel
                {
                    ChangesDescriptions = cl.ChangesDescriptions
                    ,
                    ChangedAt = cl.ChangedAt.ToString(ChangeLogDateTimeFormat),
                    ChangedBy = cl.ChangedBy
                })
                .FirstOrDefaultAsync();

            if (model == null)
            {
                return NotFound("No changes found for this person.");
            }

            return PartialView("_LatestChange", model);
        }
    }
}
