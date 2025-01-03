using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Tests
{
    public class PersonRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public PersonRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPeople()
        {
            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase("TestDb")
                .Options;

            await using (var initContext = new ApplicationDbContext(options))
            {
                initContext.Database.EnsureCreated();
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var role = new Role { Id = 1, Name = "Учител" };
                var department = new Department { Id = 1, Name = "ИТ отдел" };
                context.Roles.Add(role);
                context.Departments.Add(department);
                await context.SaveChangesAsync();

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
                context.People.Add(new Person
                {
                    Id = 2,
                    FirstName = "Мария",
                    LastName = "Иванова",
                    Email = "maria.ivanova@buditel.bg",
                    PersonalPhoneNumber = "0888333444",
                    RoleId = 1,
                    DepartmentId = 1
                });

                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(options))
            {
                var repository = new PersonRepository(context);
                var people = await repository.GetAllAsync();

                people.Should().HaveCount(2);
            }
        }


        // Test: GetByIdAsync should return person when they exist
        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var person = new Person
                {
                    Id = 1,
                    FirstName = "Иван",
                    LastName = "Петров",
                    Email = "ivan.petrov@buditel.bg",  
                    PersonalPhoneNumber = "0888123456"  
                };

                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var result = await repository.GetByIdAsync(1);

                result.Should().NotBeNull();
                result.FirstName.Should().Be("Иван");
                result.LastName.Should().Be("Петров");
            }
        }


        // Test: GetByIdAsync should return null if person does not exist
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

        // Test: AddAsync should add a person
        [Fact]
        public async Task AddAsync_ShouldAddPerson()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var person = new Person
                {
                    Id = 3,
                    FirstName = "Георги",
                    LastName = "Иванов",
                    Email = "georgi.ivanov@buditel.bg",
                    PersonalPhoneNumber = "0888123456"
                };

                await repository.AddAsync(person);

                var addedPerson = await context.People.FindAsync(3);
                addedPerson.Should().NotBeNull();
                addedPerson.FirstName.Should().Be("Георги");
                addedPerson.Email.Should().Be("georgi.ivanov@buditel.bg");
            }
        }

        // Test: UpdateAsync should modify an existing person
        [Fact]
        public async Task UpdateAsync_ShouldModifyPerson()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var person = new Person
                {
                    Id = 4,
                    FirstName = "Иван",
                    LastName = "Христов",
                    Email = "ivan.ivanov@buditel.bg",  
                    PersonalPhoneNumber = "0888777654"  
                };
                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var personToUpdate = await repository.GetByIdAsync(4);

                personToUpdate.LastName = "Иванов";
                await repository.UpdateAsync(personToUpdate);

                var updatedPerson = await context.People.FindAsync(4);
                updatedPerson.LastName.Should().Be("Иванов");
                updatedPerson.Email.Should().Be("ivan.ivanov@buditel.bg");
            }
        }


        // Test: DeleteAsync should remove a person
        [Fact]
        public async Task DeleteAsync_ShouldRemovePerson_WhenPersonExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(new Person
                {
                    Id = 5,
                    FirstName = "Стефан",
                    LastName = "Чаушев",
                    Email = "stefan.chaushev@buditel.bg",
                    PersonalPhoneNumber = "0899123456"
                });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.DeleteAsync(5);
                var deletedPerson = await context.People.FindAsync(5);
                deletedPerson.Should().BeNull();
            }
        }


        // Test: SoftDeleteAsync should mark person as deleted
        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkPersonAsDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var person = new Person
                {
                    Id = 6,
                    FirstName = "Петър",
                    LastName = "Симов",
                    Email = "petar.simov@buditel.bg",  
                    PersonalPhoneNumber = "0899988776",  
                    IsDeleted = false
                };
                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.SoftDeleteAsync(6, "напуснал");
                var softDeletedPerson = await context.People.FindAsync(6);

                softDeletedPerson.Should().NotBeNull();
                softDeletedPerson.IsDeleted.Should().BeTrue();
                softDeletedPerson.Email.Should().Be("petar.simov@buditel.bg");
            }
        }


        [Fact]
        public async Task SearchAsync_ShouldReturnMatchingPeople()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 1, Name = "Учител" };
                var department = new Department { Id = 1, Name = "ИТ отдел" };

                context.Roles.Add(role);
                context.Departments.Add(department);
                await context.SaveChangesAsync();

                context.People.Add(new Person
                {
                    Id = 7,
                    FirstName = "Даниел",
                    LastName = "Йорданов",
                    Email = "daniel.yordanov@buditel.bg",
                    PersonalPhoneNumber = "0888997766",
                    RoleId = 1,
                    DepartmentId = 1
                });
                context.People.Add(new Person
                {
                    Id = 8,
                    FirstName = "Вероника",
                    LastName = "Цачева",
                    Email = "veronika.tsacheva@buditel.bg",
                    PersonalPhoneNumber = "0888998877",
                    RoleId = 1,
                    DepartmentId = 1
                });

                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var results = await repository.SearchAsync("Даниел");

                results.Should().ContainSingle(p => p.FirstName == "Даниел");
                results.First().Email.Should().Be("daniel.yordanov@buditel.bg");
            }
        }

    }
}
