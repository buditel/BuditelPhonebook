using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Core.Repositories
{
    public class DepartmentRepository : IDepartmentRepository
    {
        private readonly ApplicationDbContext _context;

        public DepartmentRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Department>> GetAllAsync()
        {
            try
            {
                return await _context.Departments.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички отдели.", ex);
            }
        }

        public async Task<Department> GetByIdAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);

            if (department == null)
            {
                throw new KeyNotFoundException($"Отдел с ID {id} не е намерен.");
            }

            return department;
        }

        public async Task AddAsync(Department department)
        {
            try
            {
                _context.Departments.Add(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при добавяне на нов отдел.", ex);
            }
        }

        public async Task UpdateAsync(Department department)
        {
            try
            {
                _context.Departments.Update(department);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при актуализиране на отдел с ID {department.Id}.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var department = await _context.Departments.FindAsync(id);
            if (department != null)
            {
                _context.Departments.Remove(department);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var department = await _context.Departments
                .Include(d => d.PeopleDepartments)
                    .ThenInclude(pd => pd.Person)
                .FirstOrDefaultAsync(d => d.Id == id);

            if (department == null)
            {
                throw new KeyNotFoundException($"Отдел с ID {id} не е намерен.");
            }

            department.IsDeleted = true;

            try
            {
                if (!await _context.Departments.AnyAsync(d => d.Name == "Отделът е изтрит"))
                {
                    Department deletedDepartment = new Department()
                    {
                        Name = "Отделът е изтрит"
                    };

                    await AddAsync(deletedDepartment);
                }

                _context.Departments.Update(department);
                await _context.SaveChangesAsync();

                var peopleToEdit = new Dictionary<Person, string>();

                string oldDepartments = string.Empty;

                foreach (var personDepartment in department.PeopleDepartments)
                {
                    var person = personDepartment.Person;

                    oldDepartments = string.Join(", ", _context.PeopleDepartments.Where(pd => pd.PersonId == person.Id).Select(pd => pd.Department.Name).ToList());

                    if (!peopleToEdit.ContainsKey(person))
                    {
                        peopleToEdit.Add(person, oldDepartments);
                    }
                    else
                    {
                        peopleToEdit[person] = oldDepartments;
                    }

                    _context.PeopleDepartments.Remove(personDepartment);
                }

                await _context.SaveChangesAsync();

                foreach (var person in peopleToEdit)
                {
                    if (!await _context.PeopleDepartments.AnyAsync(pd => pd.PersonId == person.Key.Id))
                    {

                        var newPersonDepartment = new PersonDepartment()
                        {
                            Person = person.Key,
                            Department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == "Отделът е изтрит")
                        };

                        await _context.PeopleDepartments.AddAsync(newPersonDepartment);

                        var change = new ChangeLog
                        {
                            PersonId = person.Key.Id,
                            ChangedAt = DateTime.Now,
                            ChangedBy = "Админ",
                            ChangesDescriptions = new List<string> { $"Редактиран отдел: {person.Value} -> Изтрит отдел" }
                        };

                        await _context.ChangeLogs.AddAsync(change);
                    }
                    else
                    {
                        string newDepartments = string.Join(", ", _context.PeopleDepartments.Where(pd => pd.PersonId == person.Key.Id).Select(pd => pd.Department.Name).ToList());

                        var change = new ChangeLog
                        {
                            PersonId = person.Key.Id,
                            ChangedAt = DateTime.Now,
                            ChangedBy = "Админ",
                            ChangesDescriptions = new List<string> { $"Редактиран отдел: {person.Value} -> {newDepartments}" }
                        };

                        await _context.ChangeLogs.AddAsync(change);
                    }


                    await _context.SaveChangesAsync();
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при софт-изтриване на отдел с ID {id}.", ex);
            }
        }

        public IQueryable<Department> GetAllAttached()
        {
            try
            {
                return _context.Departments.AsQueryable();
            }
            catch (ApplicationException ex)
            {

                throw new ApplicationException("Грешка при извличане на отделите.", ex);
            }
        }
    }
}
