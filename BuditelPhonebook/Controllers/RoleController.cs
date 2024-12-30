using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
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
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(Role role)
        {
            var exists = _roleRepository.GetAllAttached().Any(r => r.Name == role.Name);
            if (exists)
            {
                ModelState.AddModelError(nameof(role.Name), NameUniqueMessage);
            }

            if (!ModelState.IsValid)
            {
                return View(role);
            }

            await _roleRepository.AddAsync(role);
            return RedirectToAction(nameof(Index));
        }

        public async Task<IActionResult> Edit(int id)
        {
            var role = await _roleRepository.GetByIdAsync(id);
            if (role == null) return NotFound();

            return View(role);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Role role)
        {
            if (!ModelState.IsValid)
                return View(role);

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
