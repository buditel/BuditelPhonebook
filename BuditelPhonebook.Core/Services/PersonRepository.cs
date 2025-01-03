using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.ViewModels;
using BuditelPhonebook.Web.ViewModels.Person;
using Microsoft.EntityFrameworkCore;

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
            return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .AsNoTracking() // Improves performance for read-only queries
                .ToListAsync();
        }

        public async Task<Person> GetByIdAsync(int id)
        {
            return await _context.People.FindAsync(id);
        }

        public async Task<Person> GetByIdWithRelationsAsync(int id)
        {
            return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task AddAsync(Person person)
        {
            _context.People.Add(person);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Person person)
        {
            _context.People.Update(person);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(int id)
        {
            var person = await _context.People.FindAsync(id);
            if (person == null)
            {
                // Optional: Log a warning that the person was not found
                return;
            }

            _context.People.Remove(person);
            await _context.SaveChangesAsync();
        }

        public async Task SoftDeleteAsync(int id, string? comment)
        {
            var person = await _context.People.FindAsync(id);

            if (person == null)
            {
                throw new ArgumentNullException(nameof(person));
            }

            person.IsDeleted = true;
            person.CommentOnDeletion = comment;

            _context.People.Update(person);
            await _context.SaveChangesAsync();
        }


        public async Task<IEnumerable<PersonDetailsViewModel>> SearchAsync(string query)
        {
            if (string.IsNullOrWhiteSpace(query))
            {
                return await _context.People
                    .Include(p => p.Role) // Include Role navigation property
                    .Include(p => p.Department)
                    .Where(p => !p.IsDeleted)
                    .Select(p => new PersonDetailsViewModel
                    {
                        Id = p.Id,
                        FirstName = p.FirstName,
                        MiddleName = p.MiddleName,
                        LastName = p.LastName,
                        Birthdate = p.Birthdate,
                        PersonalPhoneNumber = p.PersonalPhoneNumber,
                        BusinessPhoneNumber = p.BusinessPhoneNumber,
                        Email = p.Email,
                        Department = p.Department.Name,
                        Role = p.Role.Name,
                        SubjectGroup = p.SubjectGroup,
                        Subject = p.Subject,
                        PersonPicture = p.PersonPicture
                    })// Include Department navigation property
                    .ToListAsync();
            }

            query = query.ToLower();
            return await _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .Where(p => !p.IsDeleted && (p.FirstName.ToLower().Contains(query)
                            || (p.MiddleName != null && p.MiddleName.ToLower().Contains(query))
                            || p.LastName.ToLower().Contains(query)
                            || p.Email.ToLower().Contains(query)
                            || p.BusinessPhoneNumber.ToLower().Contains(query)
                            || (p.Role != null && p.Role.Name.ToLower().Contains(query)) // Ensure Role.Name is accessed
                            || (p.Department != null && p.Department.Name.ToLower().Contains(query))))
                .Select(p => new PersonDetailsViewModel
                {
                    Id = p.Id,
                    FirstName = p.FirstName,
                    MiddleName = p.MiddleName,
                    LastName = p.LastName,
                    Birthdate = p.Birthdate,
                    PersonalPhoneNumber = p.PersonalPhoneNumber,
                    BusinessPhoneNumber = p.BusinessPhoneNumber,
                    Email = p.Email,
                    Department = p.Department.Name,
                    Role = p.Role.Name,
                    SubjectGroup = p.SubjectGroup,
                    Subject = p.Subject,
                    PersonPicture = p.PersonPicture
                })// Ensure Department.Name is accessed
                .ToListAsync();
        }

        public IEnumerable<Role> GetRoles()
        {
            return _context.Roles.Where(r => !r.IsDeleted).AsNoTracking().ToList();
        }

        public IEnumerable<Department> GetDepartments()
        {
            return _context.Departments.Where(d => !d.IsDeleted).AsNoTracking().ToList();
        }

        public async Task<IEnumerable<SuggestionsViewModel>> GetSearchSuggestionsAsync(string query)
        {
            query = query.ToLower();

            var results = await _context.People
            .Include(p => p.Department)
            .Where(p => !p.IsDeleted &&
                        (p.FirstName.ToLower().Contains(query) ||
                         p.LastName.ToLower().Contains(query) ||
                         (p.Subject != null && p.Subject.ToLower().Contains(query)) ||
                         p.Department.Name.ToLower().Contains(query)))
            .Select(p => new SuggestionsViewModel
            {
                Id = p.Id,
                FullName = $"{p.FirstName} {p.LastName}",
                Subject = p.Subject,
                Department = p.Department.Name
            })
            .Take(10)
            .ToListAsync();

            return results;
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

            Person person = new Person
            {
                FirstName = model.FirstName,
                MiddleName = model.MiddleName,
                LastName = model.LastName,
                Birthdate = model.Birthdate,
                Email = model.Email,
                BusinessPhoneNumber = model.BusinessPhoneNumber,
                PersonalPhoneNumber = model.PersonalPhoneNumber,
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
            var person = await GetByIdWithRelationsAsync(id); // New repository method
            if (person == null)
            {
                //TODO:
                throw new Exception();
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

            person.FirstName = model.FirstName;
            person.MiddleName = model.MiddleName;
            person.LastName = model.LastName;
            person.PersonalPhoneNumber = model.PersonalPhoneNumber;
            person.BusinessPhoneNumber = model.BusinessPhoneNumber;
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
            return _context.People
                .Include(p => p.Role)
                .Include(p => p.Department)
                .AsQueryable();
        }
    }
}
