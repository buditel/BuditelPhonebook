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
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPeople()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

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
                    RoleId = role.Id,
                    DepartmentId = department.Id
                });
                context.People.Add(new Person
                {
                    Id = 2,
                    FirstName = "Мария",
                    LastName = "Иванова",
                    Email = "maria.ivanova@buditel.bg",
                    PersonalPhoneNumber = "0888333444",
                    RoleId = role.Id,
                    DepartmentId = department.Id
                });

                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var people = await repository.GetAllAsync();

                people.Should().HaveCount(2);
            }
        }



        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenExists()
        {
            await InitializeDatabase();

            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(CreatePerson(3, "Георги", "Иванов", "georgi@buditel.bg"));
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var result = await repository.GetByIdAsync(3);

                result.Should().NotBeNull();
                result.FirstName.Should().Be("Георги");
            }
        }

        [Fact]
        public async Task GetByIdWithRelationsAsync_ShouldReturnPersonWithRoleAndDepartment()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();

                var role = new Role { Id = 1, Name = "Учител" };
                var department = new Department { Id = 1, Name = "ИТ отдел" };

                context.Roles.Add(role);
                context.Departments.Add(department);
                await context.SaveChangesAsync();

                var person = new Person
                {
                    Id = 10,
                    FirstName = "Петър",
                    LastName = "Симеонов",
                    Email = "peter.simeonov@buditel.bg",
                    PersonalPhoneNumber = "0888123456",
                    RoleId = role.Id,
                    DepartmentId = department.Id
                };

                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var result = await repository.GetByIdWithRelationsAsync(10);

                result.Should().NotBeNull();
                result.Role.Should().NotBeNull();
                result.Department.Should().NotBeNull();
                result.Role.Name.Should().Be("Учител");
                result.Department.Name.Should().Be("ИТ отдел");
            }
        }


        [Fact]
        public async Task AddAsync_ShouldAddPerson()
        {
            await InitializeDatabase();

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var person = CreatePerson(5, "Стефан", "Колев", "stefan.kolev@buditel.bg");

                await repository.AddAsync(person);

                var addedPerson = await context.People.FirstOrDefaultAsync(p => p.Email == "stefan.kolev@buditel.bg");
                addedPerson.Should().NotBeNull();
                addedPerson.FirstName.Should().Be("Стефан");
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyPerson()
        {
            await InitializeDatabase();

            await using (var context = new ApplicationDbContext(_options))
            {
                var person = CreatePerson(6, "Анна", "Костова", "anna.kostova@buditel.bg");
                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var personToUpdate = await repository.GetByIdAsync(6);

                personToUpdate.LastName = "Петрова";
                await repository.UpdateAsync(personToUpdate);

                var updatedPerson = await context.People.FindAsync(6);
                updatedPerson.LastName.Should().Be("Петрова");
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemovePerson()
        {
            await InitializeDatabase();

            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(CreatePerson(7, "Михаил", "Караджов", "m.karadjov@buditel.bg"));
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.DeleteAsync(7);

                var deletedPerson = await context.People.FindAsync(7);
                deletedPerson.Should().BeNull();
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkPersonAsDeleted()
        {
            await InitializeDatabase();

            await using (var context = new ApplicationDbContext(_options))
            {
                var person = CreatePerson(8, "Лилия", "Ангелова", "l.angelova@buditel.bg");
                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.SoftDeleteAsync(8, "напуснал", "11.01.2021.");

                var softDeletedPerson = await context.People.FindAsync(8);
                softDeletedPerson.IsDeleted.Should().BeTrue();
            }
        }

        //[Fact]
        //public async Task SearchAsync_ShouldReturnMatchingPeople()
        //{
        //    await using (var context = new ApplicationDbContext(_options))
        //    {
        //        context.Database.EnsureDeleted();
        //        context.Database.EnsureCreated();

        //        var role = new Role { Id = 1, Name = "Учител" };
        //        var department = new Department { Id = 1, Name = "ИТ отдел" };

        //        context.Roles.Add(role);
        //        context.Departments.Add(department);
        //        await context.SaveChangesAsync();

        //        context.People.Add(new Person
        //        {
        //            Id = 7,
        //            FirstName = "Даниел",
        //            LastName = "Йорданов",
        //            Email = "daniel.yordanov@buditel.bg",
        //            PersonalPhoneNumber = "0888997766",
        //            RoleId = role.Id,
        //            DepartmentId = department.Id
        //        });

        //        context.People.Add(new Person
        //        {
        //            Id = 8,
        //            FirstName = "Вероника",
        //            LastName = "Цачева",
        //            Email = "veronika.tsacheva@buditel.bg",
        //            PersonalPhoneNumber = "0888998877",
        //            RoleId = role.Id,
        //            DepartmentId = department.Id
        //        });

        //        await context.SaveChangesAsync();
        //    }

        //    await using (var context = new ApplicationDbContext(_options))
        //    {
        //        var repository = new PersonRepository(context);
        //        var results = await repository.SearchAsync("Даниел");

        //        results.Should().ContainSingle(p => p.FirstName == "Даниел");
        //    }
        //}


        private Person CreatePerson(int id, string firstName, string lastName, string email)
        {
            return new Person
            {
                Id = id,
                FirstName = firstName,
                LastName = lastName,
                Email = email,
                PersonalPhoneNumber = "0899988776",
                RoleId = 1,
                DepartmentId = 1
            };
        }

        private async Task InitializeDatabase()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Database.EnsureDeleted();
                context.Database.EnsureCreated();
            }
        }
    }
}
