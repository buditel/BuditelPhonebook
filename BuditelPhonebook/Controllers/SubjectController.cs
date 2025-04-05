using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Subject;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using static BuditelPhonebook.Common.EntityValidationMessages.Subject;

namespace BuditelPhonebook.Web.Controllers
{
    public class SubjectController : Controller
    {
        private readonly ISubjectRepository _subjectRepository;

        public SubjectController(ISubjectRepository subjectRepository)
        {
            _subjectRepository = subjectRepository;
        }

        [Authorize(Roles = "SuperAdmin, Admin, Moderator")]
        public async Task<IActionResult> Index()
        {
            try
            {
                var subjects = await _subjectRepository.GetAllAsync();
                return View(subjects);
            }
            catch (ApplicationException)
            {
                return RedirectToAction("HttpStatusCodeHandler", "Error", new { statusCode = 500 });
            }
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        public IActionResult Create()
        {
            var model = new CreateSubjectViewModel();

            return View(model);
        }

        [Authorize(Roles = "SuperAdmin, Admin")]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateSubjectViewModel model)
        {
            try
            {
                var exists = _subjectRepository.GetAllAttached().Any(s => s.Name == model.Name);
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Name), NameUniqueMessage);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var subject = new Subject
                {
                    Name = model.Name
                };

                await _subjectRepository.AddAsync(subject);
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
                var subject = await _subjectRepository.GetByIdAsync(id);

                var model = new EditSubjectViewModel
                {
                    Id = id,
                    Name = subject.Name
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
        public async Task<IActionResult> Edit(EditSubjectViewModel model)
        {
            try
            {
                var exists = _subjectRepository.GetAllAttached().Any(s => s.Name == model.Name);
                if (exists)
                {
                    ModelState.AddModelError(nameof(model.Name), NameUniqueMessage);
                }

                if (!ModelState.IsValid)
                {
                    return View(model);
                }

                var subject = await _subjectRepository.GetByIdAsync(model.Id);
                subject.Name = model.Name;

                await _subjectRepository.UpdateAsync(subject);
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
                var subject = await _subjectRepository.GetAllAttached()
                    .Include(s => s.PeopleSubjects)
                        .ThenInclude(ps => ps.Person)
                    .FirstOrDefaultAsync(s => s.Id == id);

                return View(subject);
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
                await _subjectRepository.SoftDeleteAsync(id);
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
                var deletedSubjects = await _subjectRepository.GetAllAttached().Where(s => s.IsDeleted).ToListAsync();

                return View(deletedSubjects);
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
                var subject = await _subjectRepository.GetByIdAsync(id);

                var model = new RestoreSubjectViewModel
                {
                    Id = id,
                    Name = subject.Name
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
        public async Task<IActionResult> Restore(RestoreSubjectViewModel model)
        {
            try
            {
                var subject = await _subjectRepository.GetByIdAsync(model.Id);

                subject.IsDeleted = false;

                await _subjectRepository.UpdateAsync(subject);

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
