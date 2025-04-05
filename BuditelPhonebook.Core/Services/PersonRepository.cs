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
                .Include(p => p.PeopleRoles)
                    .ThenInclude(pr => pr.Role)
                .Include(p => p.PeopleDepartments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.PeopleSubjects)
                    .ThenInclude(ps => ps.Subject)
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
                .Include(p => p.PeopleRoles)
                    .ThenInclude(pr => pr.Role)
                .Include(p => p.PeopleDepartments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.PeopleSubjects)
                    .ThenInclude(ps => ps.Subject)
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

            person.LeaveDate = DateTime.SpecifyKind(personLeaveDate, DateTimeKind.Unspecified);
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
                .Include(p => p.PeopleRoles)
                    .ThenInclude(pr => pr.Role)
                .Include(p => p.PeopleDepartments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.PeopleSubjects)
                    .ThenInclude(ps => ps.Subject)
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
                            || (p.PeopleRoles != null && p.PeopleRoles.Any(pr => pr.Role.Name.ToLower().Contains(q)))
                            || (p.PeopleSubjects != null && p.PeopleSubjects.Any(ps => ps.Subject.Name.ToLower().Contains(q)))
                            || (p.PeopleDepartments != null && p.PeopleDepartments.Any(pd => pd.Department.Name.ToLower().Contains(q)))));
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
                        Departments = p.PeopleDepartments.Select(pd => pd.Department.Name).ToList(),
                        Roles = p.PeopleRoles.Select(pr => pr.Role.Name).ToList(),
                        SubjectGroup = p.SubjectGroup,
                        Subjects = p.PeopleSubjects.Select(ps => ps.Subject.Name).ToList(),
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
                .Include(p => p.PeopleRoles)
                    .ThenInclude(pr => pr.Role)
                .Include(p => p.PeopleDepartments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.PeopleSubjects)
                    .ThenInclude(ps => ps.Subject)
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
                            || (p.PeopleRoles != null && p.PeopleRoles.Any(pr => pr.Role.Name.ToLower().Contains(q)))
                            || (p.PeopleSubjects != null && p.PeopleSubjects.Any(ps => ps.Subject.Name.ToLower().Contains(q)))
                            || (p.PeopleDepartments != null && p.PeopleDepartments.Any(pd => pd.Department.Name.ToLower().Contains(q)))));
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
                        Departments = p.PeopleDepartments.Select(pd => pd.Department.Name).ToList(),
                        Roles = p.PeopleRoles.Select(pr => pr.Role.Name).ToList(),
                        SubjectGroup = p.SubjectGroup,
                        Subjects = p.PeopleSubjects.Select(ps => ps.Subject.Name).ToList(),
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

        public IEnumerable<Subject> GetSubjects()
        {
            try
            {
                return _context.Subjects.Where(s => !s.IsDeleted).AsNoTracking().ToList();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на предмети.", ex);
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
                SubjectGroup = model.SubjectGroup,
            };

            var selectedDepartments = model.Departments
                .Where(d => !string.IsNullOrWhiteSpace(d))
                .ToList();

            foreach (var departmentName in selectedDepartments)
            {
                PersonDepartment personDepartment = new PersonDepartment()
                {
                    Person = person,
                    Department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName)
                };

                await _context.PeopleDepartments.AddAsync(personDepartment);
                person.PeopleDepartments.Add(personDepartment);
            }

            var selectedRoles = model.Roles
                .Where(r => !string.IsNullOrWhiteSpace(r))
                .ToList();

            foreach (var roleName in selectedRoles)
            {
                PersonRole personRole = new PersonRole()
                {
                    Person = person,
                    Role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName)
                };

                await _context.PeopleRoles.AddAsync(personRole);
                person.PeopleRoles.Add(personRole);
            }


            if (model.Subjects.Any(s => s != null))
            {
                var selectedSubjects = model.Subjects
                    .Where(s => !string.IsNullOrWhiteSpace(s))
                    .ToList();

                foreach (var subjectName in selectedSubjects)
                {
                    PersonSubject personSubject = new PersonSubject()
                    {
                        Person = person,
                        Subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == subjectName)
                    };

                    await _context.PeopleSubjects.AddAsync(personSubject);
                    person.PeopleSubjects.Add(personSubject);
                }
            }

            await AddAsync(person);
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
                Departments = person.PeopleDepartments.Select(pd => pd.Department.Name).ToList(),
                Roles = person.PeopleRoles.Select(pr => pr.Role.Name).ToList(),
                SubjectGroup = person.SubjectGroup,
                Subjects = person.PeopleSubjects.Select(ps => ps.Subject.Name).ToList(),
                ExistingPicture = person.PersonPicture,
                AvailableRoles = GetRoles(),
                AvailableDepartments = GetDepartments(),
                AvailableSubjects = GetSubjects()
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

            List<PersonDepartment> peopleDepartments = new List<PersonDepartment>();

            foreach (var departmentName in model.Departments)
            {
                if (departmentName == null)
                {
                    continue;
                }

                PersonDepartment personDepartment = null;

                if (!_context.PeopleDepartments.Any(pd => pd.PersonId == person.Id && pd.Department.Name == departmentName))
                {
                    personDepartment = new PersonDepartment()
                    {
                        PersonId = person.Id,
                        Department = await _context.Departments.FirstOrDefaultAsync(d => d.Name == departmentName)
                    };

                    await _context.PeopleDepartments.AddAsync(personDepartment);
                }
                else
                {
                    personDepartment = await _context.PeopleDepartments.FirstOrDefaultAsync(pd => pd.PersonId == person.Id && pd.Department.Name == departmentName);
                }

                peopleDepartments.Add(personDepartment);
            }

            List<PersonRole> peopleRoles = new List<PersonRole>();

            foreach (var roleName in model.Roles)
            {
                if (roleName == null)
                {
                    continue;
                }

                PersonRole personRole = null;

                if (!_context.PeopleRoles.Any(pr => pr.PersonId == person.Id && pr.Role.Name == roleName))
                {
                    personRole = new PersonRole()
                    {
                        PersonId = person.Id,
                        Role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == roleName)
                    };

                    await _context.PeopleRoles.AddAsync(personRole);
                }
                else
                {
                    personRole = await _context.PeopleRoles.FirstOrDefaultAsync(pr => pr.PersonId == person.Id && pr.Role.Name == roleName);
                }

                peopleRoles.Add(personRole);
            }

            var rolesToSearch = model.Roles.Where(r => r != null).ToList();

            if (!rolesToSearch.Any(r => r.Contains("Учител")))
            {
                model.SubjectGroup = null;
                model.Subjects.Clear();

                var peopleSubjectsToDelete = await _context.PeopleSubjects.Where(pr => pr.PersonId == model.Id).ToListAsync();

                _context.PeopleSubjects.RemoveRange(peopleSubjectsToDelete);
            }

            List<PersonSubject> peopleSubjects = new List<PersonSubject>();

            foreach (var subjectName in model.Subjects)
            {
                if (subjectName == null)
                {
                    continue;
                }

                PersonSubject personSubject = null;

                if (!_context.PeopleSubjects.Any(ps => ps.PersonId == person.Id && ps.Subject.Name == subjectName))
                {
                    personSubject = new PersonSubject()
                    {
                        PersonId = person.Id,
                        Subject = await _context.Subjects.FirstOrDefaultAsync(s => s.Name == subjectName)
                    };

                    await _context.PeopleSubjects.AddAsync(personSubject);
                }
                else
                {
                    personSubject = await _context.PeopleSubjects.FirstOrDefaultAsync(ps => ps.PersonId == person.Id && ps.Subject.Name == subjectName);
                }

                peopleSubjects.Add(personSubject);
            }

            person.FirstName = model.FirstName;
            person.MiddleName = model.MiddleName;
            person.LastName = model.LastName;
            person.PersonalPhoneNumber = model.PersonalPhoneNumber;
            person.BusinessPhoneNumber = model.BusinessPhoneNumber;
            person.HireDate = hireDate;
            person.Birthdate = model.Birthdate;
            person.Email = model.Email;
            person.PeopleDepartments = peopleDepartments;
            person.PeopleRoles = peopleRoles;
            person.SubjectGroup = model.SubjectGroup;
            person.PeopleSubjects = peopleSubjects;

            await UpdateAsync(person);
        }

        public IQueryable<Person> GetAllAttached()
        {
            try
            {
                return _context.People
                .Include(p => p.PeopleRoles)
                    .ThenInclude(pr => pr.Role)
                .Include(p => p.PeopleDepartments)
                    .ThenInclude(pd => pd.Department)
                .Include(p => p.PeopleSubjects)
                    .ThenInclude(ps => ps.Subject)
                .AsQueryable();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички контакти.", ex);
            }
        }
    }
}
