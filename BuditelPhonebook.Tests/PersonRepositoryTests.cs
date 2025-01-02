using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;

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

        // Test: GetAllAsync should return all people
        [Fact]
        public async Task GetAllAsync_ShouldReturnAllPeople()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(new Person { Id = 1, FirstName = "John", LastName = "Doe" });
                context.People.Add(new Person { Id = 2, FirstName = "Jane", LastName = "Smith" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var people = await repository.GetAllAsync();
                people.Should().HaveCount(2);
            }
        }

        // Test: GetByIdAsync should return person by id
        [Fact]
        public async Task GetByIdAsync_ShouldReturnPerson_WhenPersonExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(new Person { Id = 1, FirstName = "John", LastName = "Doe" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var person = await repository.GetByIdAsync(1);
                person.Should().NotBeNull();
                person.FirstName.Should().Be("John");
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
                var person = new Person { Id = 3, FirstName = "Alice", LastName = "Cooper" };
                await repository.AddAsync(person);
                var addedPerson = await context.People.FindAsync(3);
                addedPerson.Should().NotBeNull();
                addedPerson.FirstName.Should().Be("Alice");
            }
        }

        // Test: UpdateAsync should modify an existing person
        [Fact]
        public async Task UpdateAsync_ShouldModifyPerson()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var person = new Person { Id = 4, FirstName = "Bob", LastName = "Jones" };
                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var personToUpdate = await repository.GetByIdAsync(4);
                personToUpdate.LastName = "Smith";
                await repository.UpdateAsync(personToUpdate);
                var updatedPerson = await context.People.FindAsync(4);
                updatedPerson.LastName.Should().Be("Smith");
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
                    FirstName = "Carlos",
                    LastName = "Gomez",
                    Email = "carlos@example.com",  // Добавяне на задължителното поле
                    PersonalPhoneNumber = "0899123456"  // Добавяне на задължителното поле
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
                var person = new Person { Id = 6, FirstName = "Eva", LastName = "Green", IsDeleted = false };
                context.People.Add(person);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                await repository.SoftDeleteAsync(6);
                var softDeletedPerson = await context.People.FindAsync(6);
                softDeletedPerson.Should().NotBeNull();
                softDeletedPerson.IsDeleted.Should().BeTrue();
            }
        }

        // Test: SearchAsync should return matching people by name
        [Fact]
        public async Task SearchAsync_ShouldReturnMatchingPeople()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.People.Add(new Person { Id = 7, FirstName = "Daniel", LastName = "Radcliffe" });
                context.People.Add(new Person { Id = 8, FirstName = "Emma", LastName = "Watson" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new PersonRepository(context);
                var results = await repository.SearchAsync("Daniel");
                results.Should().ContainSingle(p => p.FirstName == "Daniel");
            }
        }
    }
}
