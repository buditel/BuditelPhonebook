// Updated AdminController.cs with logger and exception handling
using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
                var person = await _personRepository.GetByIdWithRelationsAsync(id); // New repository method
                if (person == null)
                {
                    return NotFound();
                }

                var model = new EditPersonViewModel
                {
                    Id = id,
                    FirstName = person.FirstName,
                    MiddleName = person.MiddleName,
                    LastName = person.LastName,
                    Birthdate = person.Birthdate,
                    PersonalPhoneNumber = person.PersonalPhoneNumber,
                    BusinessPhoneNumber = person.BusinessPhoneNumber,
                    Email = person.Email,
                    Department = person.Department.Name,
                    Role = person.Role.Name,
                    SubjectGroup = person.SubjectGroup,
                    Subject = person.Subject,
                    ExistingPicture = person.PersonPicture,
                    Roles = _personRepository.GetRoles(),
                    Departments = _personRepository.GetDepartments()
                };

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

            var person = await _personRepository.GetByIdWithRelationsAsync(model.Id);

            try
            {

                byte[] personPictureData = null;


                if (model.PersonPicture != null)
                {
                    using MemoryStream memoryStream = new MemoryStream();
                    await model.PersonPicture.CopyToAsync(memoryStream);
                    personPictureData = memoryStream.ToArray();
                }

                if (model.PersonPicture == null && model.ExistingPicture != null)
                {
                    person.PersonPicture = model.ExistingPicture;
                }
                else
                {
                    person.PersonPicture = personPictureData;
                }

                person.FirstName = model.FirstName;
                person.MiddleName = model.MiddleName;
                person.LastName = model.LastName;
                person.PersonalPhoneNumber = model.PersonalPhoneNumber;
                person.BusinessPhoneNumber = model.BusinessPhoneNumber;
                person.Birthdate = model.Birthdate;
                person.Email = model.Email;
                person.DepartmentId = _personRepository.GetDepartments().FirstOrDefault(d => d.Name == model.Department).Id;
                person.RoleId = _personRepository.GetRoles().FirstOrDefault(r => r.Name == model.Role).Id;
                person.SubjectGroup = model.SubjectGroup;
                person.Subject = model.Subject;

                await _personRepository.UpdateAsync(person);
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error updating person with ID {person.Id} in AdminController.Edit");
                ModelState.AddModelError(string.Empty, "An error occurred while updating the person.");
                model.Roles = _personRepository.GetRoles();
                model.Departments = _personRepository.GetDepartments();
                return View(model);
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
                await _personRepository.SoftDeleteAsync(id);
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
