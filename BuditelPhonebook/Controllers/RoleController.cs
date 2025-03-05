using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var roles = await _roleRepository.GetAllAsync();
                return View(roles);
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Create()
        {
            var model = new CreateRoleViewModel();

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateRoleViewModel model)
        {
            try
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
                var role = await _roleRepository.GetByIdAsync(id);

                var model = new EditRoleViewModel
                {
                    Id = id,
                    Name = role.Name
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
        public async Task<IActionResult> Edit(EditRoleViewModel model)
        {
            try
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
                var role = await _roleRepository.GetByIdAsync(id);

                return View(role);
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
                await _roleRepository.SoftDeleteAsync(id);
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
                var deletedRoles = await _roleRepository.GetAllAttached().Where(d => d.IsDeleted).ToListAsync();

                return View(deletedRoles);
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
                var role = await _roleRepository.GetByIdAsync(id);

                var model = new RestoreRoleViewModel
                {
                    Id = id,
                    Name = role.Name
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
        public async Task<IActionResult> Restore(RestoreRoleViewModel model)
        {
            try
            {
                var role = await _roleRepository.GetByIdAsync(model.Id);

                role.IsDeleted = false;

                await _roleRepository.UpdateAsync(role);

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
