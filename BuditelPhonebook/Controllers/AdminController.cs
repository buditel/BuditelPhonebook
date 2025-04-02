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
            try
            {
                var model = new CreatePersonViewModel();

                model.AvailableRoles = _personRepository.GetRoles(); // Add a method in IPersonRepository
                model.AvailableDepartments = _personRepository.GetDepartments(); // Add a method in IPersonRepository

                // Pass a new Person instance to the view
                return View(model);
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreatePersonViewModel model)
        {
            try
            {
                var exists = _personRepository.GetAllAttached().Any(r => r.Email == model.Email);
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Email), EmailUniqueMessage);
                }

                if (model.Departments[0] == null && model.Departments.Any(d => d != null))
                {
                    var selectedDepartment = model.Departments.FirstOrDefault(d => d != null);

                    model.Departments.Remove(selectedDepartment);
                    model.Departments.Insert(0, selectedDepartment);
                }

                if (model.Departments.Any() && model.Departments[0] == null)
                {
                    ModelState.AddModelError(nameof(model.Departments), DepartmentRequiredMessage);
                }

                if (model.Roles[0] == null && model.Roles.Any(r => r != null))
                {
                    var selectedRole = model.Roles.FirstOrDefault(r => r != null);

                    model.Roles.Remove(selectedRole);
                    model.Roles.Insert(0, selectedRole);
                }

                if (model.Roles.Any() && model.Roles[0] == null)
                {
                    ModelState.AddModelError(nameof(model.Roles), RoleRequiredMessage);
                }

                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = _personRepository.GetRoles();
                    model.AvailableDepartments = _personRepository.GetDepartments();
                    model.PersonPicture = model.PersonPicture;

                    return View(model);
                }


                Person person = await _personRepository.CreateANewPerson(model);

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
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
            catch (ArgumentException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var model = await _personRepository.MapPersonForEditById(id);

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditPersonViewModel model)
        {
            try
            {
                bool exists = _personRepository.GetAllAttached().Any(r => r.Email == model.Email);
                var currentPerson = await _personRepository.GetByIdWithRelationsAsync(model.Id);

                if (exists && model.Email != currentPerson.Email)
                {
                    ModelState.AddModelError(nameof(model.Email), EmailUniqueMessage);
                }

                model.Departments = model.Departments.Where(d => !string.IsNullOrWhiteSpace(d)).ToList();

                if (model.Departments.Any())
                {
                    var selectedDepartment = model.Departments.First();
                    model.Departments.Remove(selectedDepartment);
                    model.Departments.Insert(0, selectedDepartment);
                }

                if (model.Departments.Any() && model.Departments[0] == null)
                {
                    ModelState.AddModelError(nameof(model.Departments), DepartmentRequiredMessage);
                }

                if (!ModelState.IsValid)
                {
                    model.AvailableRoles = _personRepository.GetRoles();
                    model.AvailableDepartments = _personRepository.GetDepartments();
                    return View(model);
                }

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
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(id);

                var model = new DeletePersonViewModel
                {
                    Id = id,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName
                };

                return View(model);
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(DeletePersonViewModel model)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(model.Id);
                model.FirstName = person.FirstName;
                model.MiddleName = person.MiddleName;
                model.LastName = person.LastName;

                if (model.CommentOnDeletion == null)
                {
                    return View("Delete", model);
                }

                await _personRepository.SoftDeleteAsync(model.Id, model.CommentOnDeletion, model.LeaveDate);

                return RedirectToAction("DeletedIndex");
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
            catch (ArgumentException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }

        }

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
        [HttpGet]
        public async Task<IActionResult> DeletedIndex(string search, int page = 1, int pageSize = 10)
        {
            try
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
            catch (ArgumentException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpGet]
        public async Task<IActionResult> Restore(int id)
        {
            try
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
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        public async Task<IActionResult> Restore(RestorePersonViewModel model)
        {
            try
            {
                var person = await _personRepository.GetByIdAsync(model.Id);

                person.CommentOnDeletion = null;
                person.LeaveDate = null;
                person.IsDeleted = false;

                await _personRepository.UpdateAsync(person);

                return RedirectToAction(nameof(DeletedIndex));
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
        [HttpGet]
        public async Task<IActionResult> UserRoles()
        {
            try
            {
                var userRoles = await _userRoleRepository.GetAllRolesAsync();

                var model = new UserRoleViewModel
                {
                    UserRoles = userRoles
                };

                return View(model);
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> ConfirmUserRole(UserRoleViewModel model)
        {
            try
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
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> AssignUserRole(UserRoleViewModel model)
        {
            try
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

                return RedirectToAction("UserRoles");
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpGet]
        public async Task<IActionResult> ConfirmRemoveFromRole(int id)
        {
            try
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
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
        }

        [Authorize(Roles = "SuperAdmin")]
        [HttpPost]
        public async Task<IActionResult> RemoveFromRole(int id)
        {
            try
            {
                await _userRoleRepository.RemoveRoleAsync(id);

                return RedirectToAction("UserRoles");
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        public async Task<IActionResult> SeeLatestChange(int id)
        {
            try
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
                    return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
                }

                return PartialView("_LatestChange", model);
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }
    }
}
