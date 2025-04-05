using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Core.Services
{
    public class SubjectRepository : ISubjectRepository
    {
        private readonly ApplicationDbContext _context;

        public SubjectRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Subject>> GetAllAsync()
        {
            try
            {
                return await _context.Subjects.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички предмети.", ex);
            }
        }

        public async Task<Subject> GetByIdAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);

            if (subject == null)
            {
                throw new KeyNotFoundException($"Предмет с ID {id} не е намерен.");
            }

            return subject;
        }

        public async Task AddAsync(Subject subject)
        {
            try
            {
                _context.Subjects.Add(subject);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при добавяне на нов предмет.", ex);
            }
        }


        public async Task UpdateAsync(Subject subject)
        {
            try
            {
                _context.Subjects.Update(subject);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при актуализиране на предмет с ID {subject.Id}.", ex);
            }
        }


        public async Task DeleteAsync(int id)
        {
            var subject = await _context.Subjects.FindAsync(id);
            if (subject != null)
            {
                _context.Subjects.Remove(subject);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var subject = await _context.Subjects
                .Include(s => s.PeopleSubjects)
                    .ThenInclude(pr => pr.Person)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (subject == null)
            {
                throw new KeyNotFoundException($"Предмет с ID {id} не е намерен.");
            }

            subject.IsDeleted = true;

            try
            {
                if (!await _context.Subjects.AnyAsync(r => r.Name == "Предметът е изтрит"))
                {
                    Subject deletedSubject = new Subject()
                    {
                        Name = "Предметът е изтрит"
                    };

                    await AddAsync(deletedSubject);
                }

                _context.Subjects.Update(subject);
                await _context.SaveChangesAsync();

                var peopleToEdit = new Dictionary<Person, string>();

                string oldSubjects = string.Empty;

                foreach (var personSubject in subject.PeopleSubjects)
                {
                    var person = personSubject.Person;

                    oldSubjects = string.Join(", ", _context.PeopleSubjects.Where(ps => ps.PersonId == person.Id).Select(ps => ps.Subject.Name).ToList());

                    if (!peopleToEdit.ContainsKey(person))
                    {
                        peopleToEdit.Add(person, oldSubjects);
                    }
                    else
                    {
                        peopleToEdit[person] = oldSubjects;
                    }

                    _context.PeopleSubjects.Remove(personSubject);
                }

                await _context.SaveChangesAsync();

                foreach (var person in peopleToEdit)
                {
                    if (!await _context.PeopleSubjects.AnyAsync(ps => ps.PersonId == person.Key.Id))
                    {

                        var newPersonSubject = new PersonSubject()
                        {
                            Person = person.Key,
                            Subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == "Предметът е изтрит")
                        };

                        await _context.PeopleSubjects.AddAsync(newPersonSubject);

                        var change = new ChangeLog
                        {
                            PersonId = person.Key.Id,
                            ChangedAt = DateTime.Now,
                            ChangedBy = "Админ",
                            ChangesDescriptions = new List<string> { $"Редактиран предмет: {person.Value} -> Изтрит предмет" }
                        };

                        await _context.ChangeLogs.AddAsync(change);
                    }
                    else
                    {
                        string newSubjects = string.Join(", ", _context.PeopleSubjects.Where(ps => ps.PersonId == person.Key.Id).Select(ps => ps.Subject.Name).ToList());

                        var change = new ChangeLog
                        {
                            PersonId = person.Key.Id,
                            ChangedAt = DateTime.Now,
                            ChangedBy = "Админ",
                            ChangesDescriptions = new List<string> { $"Редактиран предмет: {person.Value} -> {newSubjects}" }
                        };

                        await _context.ChangeLogs.AddAsync(change);
                    }


                    await _context.SaveChangesAsync();
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при софт-изтриване на предмет с ID {id}.", ex);
            }
        }

        public IQueryable<Subject> GetAllAttached()
        {
            try
            {
                return _context.Subjects.AsQueryable();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на предметите.", ex);
            }
        }
    }
}
