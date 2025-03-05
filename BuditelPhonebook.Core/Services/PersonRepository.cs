using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels.Person;
using Microsoft.EntityFrameworkCore;
using System.Globalization;
using static BuditelPhonebook.Common.EntityValidationConstants.Person;

namespace BuditelPhonebook.Core.Repositories
{
    public class PersonRepository : IPersonRepository
    {
        private readonly ApplicationDbContext _context;

        public PersonRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Person>> GetAllAsync()
        {
            try
            {
                return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .AsNoTracking() // Improves performance for read-only queries
                .ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички контакти.", ex);
            }

        }

        public async Task<Person> GetByIdAsync(int id)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null)
            {
                throw new KeyNotFoundException($"Контакт с ID {id} не е намерен.");
            }

            return person;
        }

        public async Task<Person> GetByIdWithRelationsAsync(int id)
        {
            var person = await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (person == null)
            {
                throw new KeyNotFoundException($"Контакт с ID {id} не е намерен.");
            }

            return person;
        }

        public async Task AddAsync(Person person)
        {
            try
            {
                person.HireDate = DateTime.SpecifyKind(person.HireDate, DateTimeKind.Utc);

                await _context.People.AddAsync(person);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при добавяне на нов контакт.", ex);
            }
        }

        public async Task UpdateAsync(Person person)
        {
            try
            {
                person.HireDate = DateTime.SpecifyKind(person.HireDate, DateTimeKind.Utc);

                _context.People.Update(person);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при актуализиране на контакт с ID {person.Id}.", ex);
            }
        }

        public async Task DeleteAsync(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                return;
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id, string? comment, string leaveDate)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null)
            {
                throw new KeyNotFoundException($"Контакт с ID {id} не е намерен.");
            }

            bool isDateValid = DateTime.TryParseExact(leaveDate, HireAndLeaveDateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime personLeaveDate);

            if (!isDateValid)
            {
                throw new ArgumentException("Форматът на датата е невалиден.");
            }

            person.LeaveDate = DateTime.SpecifyKind(personLeaveDate, DateTimeKind.Utc);
            person.IsDeleted = true;
            person.CommentOnDeletion = comment;

            try
            {
                _context.People.Update(person);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при софт-изтриване на контакт с ID {id}.", ex)
                    ;
            }
        }


        public async Task<(IEnumerable<PersonDetailsViewModel> People, int TotalCount)> SearchAsync(string query, int page = 1, int pageSize = 10)
        {
            if (page < 1)
            {
                throw new ArgumentException("Невалиден номер на страница.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("Невалиден размер на страница.");
            }

            try
            {
                IQueryable<Person> queryable = _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .Include(p => p.ChangeLogs)
                .Where(p => !p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(query) && query.Length > 1)
                {
                    var queryArray = query.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    queryable = queryable.Where(p =>
                        queryArray.All(q =>
                            p.FirstName.ToLower().Contains(q)
                            || p.LastName.ToLower().Contains(q)
                            || p.Email.ToLower().Contains(q)
                            || (p.BusinessPhoneNumber != null && p.BusinessPhoneNumber.ToLower().Contains(q))
                            || p.PersonalPhoneNumber.ToLower().Contains(q)
                            || (p.Role != null && p.Role.Name.ToLower().Contains(q))
                            || (p.Subject != null && p.Subject.ToLower().Contains(q))
                            || (p.Department != null && p.Department.Name.ToLower().Contains(q))));
                }

                int totalCount = await queryable.CountAsync();

                var people = await queryable
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(p => new PersonDetailsViewModel
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        MiddleName = p.MiddleName,
                        LastName = p.LastName,
                        Birthdate = p.Birthdate,
                        PersonalPhoneNumber = p.PersonalPhoneNumber,
                        BusinessPhoneNumber = p.BusinessPhoneNumber,
                        HireDate = p.HireDate.ToString(HireAndLeaveDateFormat),
                        Email = p.Email,
                        Department = p.Department.Name,
                        Role = p.Role.Name,
                        SubjectGroup = p.SubjectGroup,
                        Subject = p.Subject,
                        PersonPicture = p.PersonPicture,
                        ChangedAt = p.ChangeLogs.OrderByDescending(c => c.ChangedAt).FirstOrDefault().ChangedAt.ToString(HireAndLeaveDateFormat)
                    })
                    .ToListAsync();

                return (people, totalCount);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на контакти", ex);
            }

        }

        public async Task<(IEnumerable<DeletedIndexPersonViewModel> People, int TotalCount)> SearchDeletedAsync(string query, int page = 1, int pageSize = 10)
        {
            if (page < 1)
            {
                throw new ArgumentException("Невалиден номер на страница.");
            }

            if (pageSize < 1)
            {
                throw new ArgumentException("Невалиден размер на страница.");
            }

            try
            {
                IQueryable<Person> queryable = _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .Include(p => p.ChangeLogs)
                .Where(p => p.IsDeleted);

                if (!string.IsNullOrWhiteSpace(query) && query.Length > 1)
                {
                    var queryArray = query.ToLower().Split(" ", StringSplitOptions.RemoveEmptyEntries);
                    queryable = queryable.Where(p =>
                        queryArray.All(q =>
                            p.FirstName.ToLower().Contains(q)
                            || p.LastName.ToLower().Contains(q)
                            || p.Email.ToLower().Contains(q)
                            || (p.BusinessPhoneNumber != null && p.BusinessPhoneNumber.ToLower().Contains(q))
                            || p.PersonalPhoneNumber.ToLower().Contains(q)
                            || (p.Role != null && p.Role.Name.ToLower().Contains(q))
                            || (p.Subject != null && p.Subject.ToLower().Contains(q))
                            || (p.Department != null && p.Department.Name.ToLower().Contains(q))));
                }

                int totalCount = await queryable.CountAsync();

                var peopleList = await queryable
                    .OrderBy(p => p.FirstName)
                    .ThenBy(p => p.LastName)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .ToListAsync();

                var people = peopleList
                    .Select(p => new DeletedIndexPersonViewModel
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        MiddleName = p.MiddleName,
                        LastName = p.LastName,
                        Birthdate = p.Birthdate,
                        PersonalPhoneNumber = p.PersonalPhoneNumber,
                        BusinessPhoneNumber = p.BusinessPhoneNumber,
                        HireDate = p.HireDate.ToString(HireAndLeaveDateFormat),
                        LeaveDate = p.LeaveDate?.ToString(HireAndLeaveDateFormat),
                        CommentOnDeletion = p.CommentOnDeletion,
                        Email = p.Email,
                        Department = p.Department.Name,
                        Role = p.Role.Name,
                        SubjectGroup = p.SubjectGroup,
                        Subject = p.Subject,
                        PersonPicture = p.PersonPicture,
                    })
                    .ToList();

                return (people, totalCount);
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на изтрити контакти", ex);
            }

        }

        public IEnumerable<Role> GetRoles()
        {
            try
            {
                return _context.Roles.Where(r => !r.IsDeleted).AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на длъжности.", ex);
            }
        }

        public IEnumerable<Department> GetDepartments()
        {
            try
            {
                return _context.Departments.Where(d => !d.IsDeleted).AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на отдели.", ex);
            }
        }

        public async Task<Person> CreateANewPerson(CreatePersonViewModel model)
        {
            byte[] personPictureData = null;

            if (model.PersonPicture != null)
            {
                using MemoryStream memoryStream = new MemoryStream();
                await model.PersonPicture.CopyToAsync(memoryStream);
                personPictureData = memoryStream.ToArray();
            }

            if (model.PersonalPhoneNumber.StartsWith("+359"))
            {
                model.PersonalPhoneNumber = model.PersonalPhoneNumber.Replace("+359", "0");
            }

            if (model.BusinessPhoneNumber != null && model.BusinessPhoneNumber.StartsWith("+359"))
            {
                model.BusinessPhoneNumber = model.BusinessPhoneNumber.Replace("+359", "0");
            }

            bool isDateValid = DateTime.TryParseExact(model.HireDate, HireAndLeaveDateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime hireDate);

            if (!isDateValid)
            {
                throw new ArgumentException("Форматът на датата е невалиден.");
            }

            Person person = new Person
            {
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Birthdate = model.Birthdate,
                Email = model.Email,
                BusinessPhoneNumber = model.BusinessPhoneNumber,
                PersonalPhoneNumber = model.PersonalPhoneNumber,
                HireDate = hireDate,
                PersonPicture = personPictureData,
                DepartmentId = GetDepartments().FirstOrDefault(d => d.Name == model.Department).Id,
                RoleId = GetRoles().FirstOrDefault(r => r.Name == model.Role).Id,
                SubjectGroup = model.SubjectGroup,
                Subject = model.Subject
            };

            return person;
        }

        public async Task<EditPersonViewModel> MapPersonForEditById(int id)
        {
            var person = await GetByIdWithRelationsAsync(id);
            if (person == null)
            {
                throw new KeyNotFoundException($"Контакт с ID {id} не е намерен.");
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
                HireDate = person.HireDate.ToString(HireAndLeaveDateFormat),
                Email = person.Email,
                Department = person.Department.Name,
                Role = person.Role.Name,
                SubjectGroup = person.SubjectGroup,
                Subject = person.Subject,
                ExistingPicture = person.PersonPicture,
                Roles = GetRoles(),
                Departments = GetDepartments()
            };

            return model;
        }

        public async Task EditPerson(EditPersonViewModel model)
        {
            var person = await GetByIdWithRelationsAsync(model.Id);

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
            else if (model.PersonPicture == null && model.ExistingPicture == null)
            {
                person.PersonPicture = model.ExistingPicture;
            }
            else
            {
                person.PersonPicture = personPictureData;
            }

            if (model.PersonalPhoneNumber.StartsWith("+359"))
            {
                model.PersonalPhoneNumber = model.PersonalPhoneNumber.Replace("+359", "0");
            }

            if (model.BusinessPhoneNumber != null && model.BusinessPhoneNumber.StartsWith("+359"))
            {
                model.BusinessPhoneNumber = model.BusinessPhoneNumber.Replace("+359", "0");
            }

            bool isDateValid = DateTime.TryParseExact(model.HireDate, HireAndLeaveDateFormat, CultureInfo.CurrentCulture, DateTimeStyles.None, out DateTime hireDate);

            if (!isDateValid)
            {
                throw new ArgumentException("Форматът на датата е невалиден.");
            }

            person.FirstName = model.FirstName;
            person.MiddleName = model.MiddleName;
            person.LastName = model.LastName;
            person.PersonalPhoneNumber = model.PersonalPhoneNumber;
            person.BusinessPhoneNumber = model.BusinessPhoneNumber;
            person.HireDate = hireDate;
            person.Birthdate = model.Birthdate;
            person.Email = model.Email;
            person.DepartmentId = GetDepartments().FirstOrDefault(d => d.Name == model.Department).Id;
            person.RoleId = GetRoles().FirstOrDefault(r => r.Name == model.Role).Id;
            person.SubjectGroup = model.SubjectGroup;
            person.Subject = model.Subject;

            await UpdateAsync(person);
        }

        public IQueryable<Person> GetAllAttached()
        {
            try
            {
                return _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .AsQueryable();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички контакти.", ex);
            }
        }
    }
}
