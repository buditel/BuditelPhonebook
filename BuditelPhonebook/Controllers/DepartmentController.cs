using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Department;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BuditelPhonebook.Common.EntityValidationMessages.Department;

namespace BuditelPhonebook.Web.Controllers
{
    public class DepartmentController : Controller
    {
        private readonly IDepartmentRepository _departmentRepository;

        public DepartmentController(IDepartmentRepository departmentRepository)
        {
            _departmentRepository = departmentRepository;
        }

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var departments = await _departmentRepository.GetAllAsync();
                return View(departments);
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Create()
        {
            CreateDepartmentViewModel model = new CreateDepartmentViewModel();

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentViewModel model)
        {
            try
            {
                var exists = _departmentRepository.GetAllAttached().Any(d => d.Name == model.Name);
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Name), NameUniqueMessage);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var department = new Department
                {
                    Name = model.Name
                };


                await _departmentRepository.AddAsync(department);
                return RedirectToAction(nameof(Index));
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(id);

                var model = new EditDepartmentViewModel
                {
                    Id = id,
                    Name = department.Name
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
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditDepartmentViewModel model)
        {
            try
            {
                var exists = _departmentRepository.GetAllAttached().Any(d => d.Name == model.Name);
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Name), NameUniqueMessage);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var department = await _departmentRepository.GetByIdAsync(model.Id);

                department.Name = model.Name;

                await _departmentRepository.UpdateAsync(department);
                return RedirectToAction(nameof(Index));
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

        [Authorize(Roles = "SuperAdmin, Admin")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(id);

                return View(department);
            }
            catch (KeyNotFoundException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 404 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            try
            {
                await _departmentRepository.SoftDeleteAsync(id);
                return RedirectToAction(nameof(Index));
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
        public async Task<IActionResult> DeletedIndex()
        {
            try
            {
                var deletedDepartments = await _departmentRepository.GetAllAttached().Where(d => d.IsDeleted).ToListAsync();

                return View(deletedDepartments);
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
                var department = await _departmentRepository.GetByIdAsync(id);

                var model = new RestoreDepartmentViewModel
                {
                    Id = id,
                    Name = department.Name
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
        public async Task<IActionResult> Restore(RestoreDepartmentViewModel model)
        {
            try
            {
                var department = await _departmentRepository.GetByIdAsync(model.Id);

                department.IsDeleted = false;

                await _departmentRepository.UpdateAsync(department);

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
    }
}
