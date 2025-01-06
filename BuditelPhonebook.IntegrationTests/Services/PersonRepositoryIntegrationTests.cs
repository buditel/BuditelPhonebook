using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Tests.Integration;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Tests.Integration
{
    public class PersonRepositoryIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public PersonRepositoryIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPeople()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 1, Name = "Admin" };
                var department = new Department { Id = 1, Name = "IT" };

                context.Roles.Add(role);
                context.Departments.Add(department);
                context.People.Add(new Person
                {
                    Id = 1,
                    FirstName = "Иван",
                    LastName = "Петров",
                    Email = "ivan.petrov@buditel.bg",
                    PersonalPhoneNumber = "0888123456",
                    RoleId = 1,
                    DepartmentId = 1
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var people = await repository.GetAllAsync();

                people.Should().HaveCount(1);
                people.First().FirstName.Should().Be("Иван");
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(new Person
                {
                    Id = 2,
                    FirstName = "Мария",
                    LastName = "Иванова",
                    Email = "maria.ivanova@buditel.bg",
                    PersonalPhoneNumber = "0888333444"
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var person = await repository.GetByIdAsync(2);

                person.Should().NotBeNull();
                person.FirstName.Should().Be("Мария");
            }
        }

        [Fact]
        public async Task GetByIdWithRelationsAsync_ShouldReturnPersonWithRoleAndDepartment()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 3, Name = "Teacher" };
                var department = new Department { Id = 2, Name = "Education" };

                context.Roles.Add(role);
                context.Departments.Add(department);

                context.People.Add(new Person
                {
                    Id = 3,
                    FirstName = "Даниел",
                    LastName = "Йорданов",
                    Email = "daniel.yordanov@buditel.bg",
                    PersonalPhoneNumber = "0888123456",
                    RoleId = 3,
                    DepartmentId = 2
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var person = await repository.GetByIdWithRelationsAsync(3);

                person.Should().NotBeNull();
                person.Role.Name.Should().Be("Teacher");
                person.Department.Name.Should().Be("Education");
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkPersonAsDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(new Person
                {
                    Id = 4,
                    FirstName = "Петър",
                    LastName = "Симеонов",
                    Email = "peter.simeonov@buditel.bg",
                    PersonalPhoneNumber = "0899123456",
                    IsDeleted = false
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.SoftDeleteAsync(4, "напуснал");

                var deletedPerson = await context.People.FindAsync(4);
                deletedPerson.Should().NotBeNull();
                deletedPerson.IsDeleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenPersonIsNull()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                Func<Task> act = async () => await repository.AddAsync(null);
                await act.Should().ThrowAsync<NullReferenceException>();
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenPersonIsNull()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                Func<Task> act = async () => await repository.UpdateAsync(null);

                await act.Should().ThrowAsync<NullReferenceException>();
            }
        }


        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_WhenPersonDoesNotExist()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(999));
                exception.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenPersonDoesNotExist()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var person = await repository.GetByIdAsync(999);
                person.Should().BeNull();
            }
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnMatchingPeople_ByName()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 5, Name = "Developer" };
                var department = new Department { Id = 6, Name = "Engineering" };

                context.Roles.Add(role);
                context.Departments.Add(department);

                context.People.Add(new Person
                {
                    Id = 5,
                    FirstName = "Алекс",
                    LastName = "Петров",
                    Email = "alex.petrov@buditel.bg",
                    PersonalPhoneNumber = "0888111222",
                    RoleId = 5,
                    DepartmentId = 6
                });
                context.People.Add(new Person
                {
                    Id = 6,
                    FirstName = "Александър",
                    LastName = "Георгиев",
                    Email = "alex.georgiev@buditel.bg",
                    PersonalPhoneNumber = "0888333444",
                    RoleId = 5,
                    DepartmentId = 6
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var results = await repository.SearchAsync("Алекс");

                results.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task SearchAsync_ShouldReturnEmpty_WhenNoMatch()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var results = await repository.SearchAsync("Няма такъв");
                results.Should().BeEmpty();
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldNotThrow_WhenPersonAlreadyDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 5, Name = "Developer" };
                var department = new Department { Id = 6, Name = "Engineering" };

                context.People.Add(new Person
                {
                    Id = 7,
                    FirstName = "Кирил",
                    LastName = "Димитров",
                    Email = "kiril.dimitrov@buditel.bg",
                    PersonalPhoneNumber = "0888333444",
                    RoleId = 5,
                    DepartmentId = 6,
                    IsDeleted = true
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.SoftDeleteAsync(7, "напуснал");

                var deletedPerson = await context.People.FindAsync(7);
                deletedPerson.IsDeleted.Should().BeTrue();
            }
        }

    }
}
