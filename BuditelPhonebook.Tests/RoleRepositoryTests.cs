using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

public class RoleRepositoryTests
{
    private readonly DbContextOptions<ApplicationDbContext> _options;

    public RoleRepositoryTests()
    {
        _options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase("TestDb")
            .Options;
    }

    private async Task ClearDatabase(ApplicationDbContext context)
    {
        context.Roles.RemoveRange(context.Roles);
        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRoles()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            context.Roles.Add(new Role { Id = 1, Name = "Admin" });
            context.Roles.Add(new Role { Id = 2, Name = "User" });
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var roles = await repository.GetAllAsync();
            roles.Should().HaveCount(2);
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            context.Roles.Add(new Role { Id = 1, Name = "Admin" });
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var role = await repository.GetByIdAsync(1);
            role.Should().NotBeNull();
            role.Name.Should().Be("Admin");
        }
    }

    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var repository = new RoleRepository(context);
            var role = await repository.GetByIdAsync(999);
            role.Should().BeNull();
        }
    }

    [Fact]
    public async Task AddAsync_ShouldAddRole()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var repository = new RoleRepository(context);
            var role = new Role { Id = 3, Name = "Manager" };
            await repository.AddAsync(role);

            var addedRole = await context.Roles.FindAsync(3);
            addedRole.Should().NotBeNull();
            addedRole.Name.Should().Be("Manager");
        }
    }

    [Fact]
    public async Task AddAsync_ShouldThrow_WhenRoleIsNull()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var repository = new RoleRepository(context);
            Func<Task> act = async () => await repository.AddAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldModifyRole()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var role = new Role { Id = 4, Name = "Operator" };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var roleToUpdate = await repository.GetByIdAsync(4);
            roleToUpdate.Name = "Supervisor";
            await repository.UpdateAsync(roleToUpdate);

            var updatedRole = await context.Roles.FindAsync(4);
            updatedRole.Name.Should().Be("Supervisor");
        }
    }

    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenRoleIsNull()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var repository = new RoleRepository(context);
            Func<Task> act = async () => await repository.UpdateAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldRemoveRole_WhenRoleExists()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            context.Roles.Add(new Role { Id = 5, Name = "TempRole" });
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            await repository.DeleteAsync(5);
            var deletedRole = await context.Roles.FindAsync(5);
            deletedRole.Should().BeNull();
        }
    }

    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenRoleDoesNotExist()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var repository = new RoleRepository(context);
            var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(999));
            exception.Should().BeNull();
        }
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkRoleAsDeleted()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var role = new Role { Id = 8, Name = "SoftDeleteRole", IsDeleted = false };
            context.Roles.Add(role);
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            await repository.SoftDeleteAsync(8);

            var softDeletedRole = await context.Roles.FindAsync(8);
            softDeletedRole.Should().NotBeNull();
            softDeletedRole.IsDeleted.Should().BeTrue();
        }
    }

    [Fact]
    public async Task SoftDeleteAsync_ShouldThrow_WhenRoleDoesNotExist()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            await ClearDatabase(context);

            var repository = new RoleRepository(context);
            Func<Task> act = async () => await repository.SoftDeleteAsync(999);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }
}
