using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Department;
using Microsoft.AspNetCore.Mvc;

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

        public async Task<IActionResult> Index()
        {
            var departments = await _departmentRepository.GetAllAsync();
            return View(departments);
        }

        public IActionResult Create()
        {
            CreateDepartmentViewModel model = new CreateDepartmentViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateDepartmentViewModel model)
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

        public async Task<IActionResult> Edit(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null)
            {
                return NotFound();
            }

            var model = new EditDepartmentViewModel
            {
                Id = id,
                Name = department.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditDepartmentViewModel model)
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

        public async Task<IActionResult> Delete(int id)
        {
            var department = await _departmentRepository.GetByIdAsync(id);
            if (department == null) return NotFound();

            return View(department);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _departmentRepository.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
