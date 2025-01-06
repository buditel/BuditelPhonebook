using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class PersonWithRoleAndDepartmentIntegrationTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public PersonWithRoleAndDepartmentIntegrationTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
    }

    [Fact]
    public async Task AddPersonWithRoleAndDepartment_ShouldSaveAll()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var roleRepo = new RoleRepository(context);
            var departmentRepo = new DepartmentRepository(context);
            var personRepo = new PersonRepository(context);

            // Arrange
            var role = new Role { Id = 1, Name = "Developer" };
            var department = new Department { Id = 1, Name = "Engineering" };

            await roleRepo.AddAsync(role);
            await departmentRepo.AddAsync(department);

            var person = new Person
            {
                Id = 1,
                FirstName = "Алекс",
                LastName = "Петров",
                Email = "alex.petrov@buditel.bg",
                PersonalPhoneNumber = "0888111222",
                RoleId = 1,
                DepartmentId = 1
            };

            // Act
            await personRepo.AddAsync(person);

            // Assert
            var savedPerson = await personRepo.GetByIdWithRelationsAsync(1);
            savedPerson.Should().NotBeNull();
            savedPerson.Role.Name.Should().Be("Developer");
            savedPerson.Department.Name.Should().Be("Engineering");
        }
    }
}
