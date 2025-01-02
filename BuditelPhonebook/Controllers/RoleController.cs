using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Role;
using Microsoft.AspNetCore.Mvc;

using static BuditelPhonebook.Common.EntityValidationMessages.Role;

namespace BuditelPhonebook.Web.Controllers
{
    public class RoleController : Controller
    {
        private readonly IRoleRepository _roleRepository;

        public RoleController(IRoleRepository roleRepository)
        {
            _roleRepository = roleRepository;
        }

        public async Task<IActionResult> Index()
        {
            var roles = await _roleRepository.GetAllAsync();
            return View(roles);
        }

        public IActionResult Create()
        {
            var model = new CreateRoleViewModel();

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            var exists = _roleRepository.GetAllAttached().Any(r => r.Name == model.Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), NameUniqueMessage);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = new Role
            {
                Name = model.Name
            };

            await _roleRepository.AddAsync(role);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null)
            {
                return NotFound();
            }

            var model = new EditRoleViewModel
            {
                Id = id,
                Name = role.Name
            };

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            var exists = _roleRepository.GetAllAttached().Any(d => d.Name == model.Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(model.Name), NameUniqueMessage);
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var role = await _roleRepository.GetByIdAsync(model.Id);
            role.Name = model.Name;

            await _roleRepository.UpdateAsync(role);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Delete(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            await _roleRepository.SoftDeleteAsync(id);
            return RedirectToAction(nameof(Index));
        }
    }
}
