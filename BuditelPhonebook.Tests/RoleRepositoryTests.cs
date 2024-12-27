using BuditelPhonebook.Data;
using BuditelPhonebook.Models;
using BuditelPhonebook.Services;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using System.Threading.Tasks;

public class RoleRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public RoleRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
    }

    // Test: GetAllAsync should return all roles
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRoles()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            context.Roles.Add(new Role { Id = 1, Name = "Admin" });
            context.Roles.Add(new Role { Id = 2, Name = "User" });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var roles = await repository.GetAllAsync();
            roles.Should().HaveCount(2);
        }
    }

    // Test: GetByIdAsync should return correct role
    [Fact]
    public async Task GetByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            context.Roles.Add(new Role { Id = 1, Name = "Admin" });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var role = await repository.GetByIdAsync(1);
            role.Should().NotBeNull();
            role.Name.Should().Be("Admin");
        }
    }

    // Test: GetByIdAsync should return null for non-existing role
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var role = await repository.GetByIdAsync(999);
            role.Should().BeNull();
        }
    }

    // Test: AddAsync should add a role
    [Fact]
    public async Task AddAsync_ShouldAddRole()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var role = new Role { Id = 3, Name = "Manager" };
            await repository.AddAsync(role);
            var addedRole = await context.Roles.FindAsync(3);
            addedRole.Should().NotBeNull();
            addedRole.Name.Should().Be("Manager");
        }
    }

    // Edge Case: AddAsync should not add null role
    [Fact]
    public async Task AddAsync_ShouldThrow_WhenRoleIsNull()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);

            Func<Task> act = async () => await repository.AddAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }


    // Test: UpdateAsync should modify an existing role
    [Fact]
    public async Task UpdateAsync_ShouldModifyRole()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var role = new Role { Id = 4, Name = "Operator" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var roleToUpdate = await repository.GetByIdAsync(4);
            roleToUpdate.Name = "Supervisor";
            await repository.UpdateAsync(roleToUpdate);
            var updatedRole = await context.Roles.FindAsync(4);
            updatedRole.Name.Should().Be("Supervisor");
        }
    }

    // Edge Case: UpdateAsync should not update null role
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenRoleIsNull()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);

            Func<Task> act = async () => await repository.UpdateAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }


    // Test: DeleteAsync should remove a role
    [Fact]
    public async Task DeleteAsync_ShouldRemoveRole_WhenRoleExists()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            context.Roles.Add(new Role { Id = 5, Name = "TempRole" });
            await context.SaveChangesAsync();
        }

        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            await repository.DeleteAsync(5);
            var deletedRole = await context.Roles.FindAsync(5);
            deletedRole.Should().BeNull();
        }
    }

    // Edge Case: DeleteAsync should not throw for non-existing role
    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenRoleDoesNotExist()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(999));
            exception.Should().BeNull();
        }
    }

    // Concurrency Test: Simulate concurrent role additions
    [Fact]
    public async Task AddAsync_ShouldHandleConcurrentRequests()
    {
        using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);

            var task1 = repository.AddAsync(new Role { Id = 6, Name = "Engineer" });
            var task2 = repository.AddAsync(new Role { Id = 7, Name = "Technician" });

            await Task.WhenAll(task1, task2);

            var roles = await repository.GetAllAsync();
            roles.Should().ContainSingle(r => r.Name == "Engineer");
            roles.Should().ContainSingle(r => r.Name == "Technician");
        }
    }
}
