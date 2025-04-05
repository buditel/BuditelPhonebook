using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Core.Repositories
{
    public class RoleRepository : IRoleRepository
    {
        private readonly ApplicationDbContext _context;

        public RoleRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Role>> GetAllAsync()
        {
            try
            {
                return await _context.Roles.ToListAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на всички длъжности.", ex);
            }
        }

        public async Task<Role> GetByIdAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);

            if (role == null)
            {
                throw new KeyNotFoundException($"Длъжност с ID {id} не е намерена.");
            }

            return role;
        }

        public async Task AddAsync(Role role)
        {
            try
            {
                _context.Roles.Add(role);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при добавяне на нова длъжност.", ex);
            }
        }


        public async Task UpdateAsync(Role role)
        {
            try
            {
                _context.Roles.Update(role);
                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при актуализиране на длъжност с ID {role.Id}.", ex);
            }
        }


        public async Task DeleteAsync(int id)
        {
            var role = await _context.Roles.FindAsync(id);
            if (role != null)
            {
                _context.Roles.Remove(role);
                await _context.SaveChangesAsync();
            }
        }

        public async Task SoftDeleteAsync(int id)
        {
            var role = await _context.Roles
                .Include(r => r.PeopleRoles)
                    .ThenInclude(pr => pr.Person)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (role == null)
            {
                throw new KeyNotFoundException($"Длъжност с ID {id} не е намерена.");
            }

            role.IsDeleted = true;

            try
            {
                if (!await _context.Roles.AnyAsync(r => r.Name == "Длъжността е изтрита"))
                {
                    Role deletedRole = new Role()
                    {
                        Name = "Длъжността е изтрита"
                    };

                    await AddAsync(deletedRole);
                }

                _context.Roles.Update(role);
                await _context.SaveChangesAsync();

                var peopleToEdit = new Dictionary<Person, string>();

                string oldRoles = string.Empty;

                foreach (var personRole in role.PeopleRoles)
                {
                    var person = personRole.Person;

                    oldRoles = string.Join(", ", _context.PeopleRoles.Where(pr => pr.PersonId == person.Id).Select(pr => pr.Role.Name).ToList());

                    if (!peopleToEdit.ContainsKey(person))
                    {
                        peopleToEdit.Add(person, oldRoles);
                    }
                    else
                    {
                        peopleToEdit[person] = oldRoles;
                    }

                    _context.PeopleRoles.Remove(personRole);
                }

                await _context.SaveChangesAsync();

                foreach (var person in peopleToEdit)
                {
                    if (!await _context.PeopleRoles.AnyAsync(pr => pr.PersonId == person.Key.Id))
                    {

                        var newPersonRole = new PersonRole()
                        {
                            Person = person.Key,
                            Role = await _context.Roles.FirstOrDefaultAsync(r => r.Name == "Длъжността е изтрита")
                        };

                        await _context.PeopleRoles.AddAsync(newPersonRole);

                        var change = new ChangeLog
                        {
                            PersonId = person.Key.Id,
                            ChangedAt = DateTime.Now,
                            ChangedBy = "Админ",
                            ChangesDescriptions = new List<string> { $"Редактирана длъжност: {person.Value} -> Изтрита длъжност" }
                        };

                        await _context.ChangeLogs.AddAsync(change);
                    }
                    else
                    {
                        string newRoles = string.Join(", ", _context.PeopleRoles.Where(pr => pr.PersonId == person.Key.Id).Select(pr => pr.Role.Name).ToList());

                        var change = new ChangeLog
                        {
                            PersonId = person.Key.Id,
                            ChangedAt = DateTime.Now,
                            ChangedBy = "Админ",
                            ChangesDescriptions = new List<string> { $"Редактирана длъжност: {person.Value} -> {newRoles}" }
                        };

                        await _context.ChangeLogs.AddAsync(change);
                    }


                    await _context.SaveChangesAsync();
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                throw new ApplicationException($"Грешка при софт-изтриване на длъжност с ID {id}.", ex);
            }
        }

        public IQueryable<Role> GetAllAttached()
        {
            try
            {
                return _context.Roles.AsQueryable();
            }
            catch (Exception ex)
            {
                throw new ApplicationException("Грешка при извличане на длъжностите.", ex);
            }
        }
    }

}
