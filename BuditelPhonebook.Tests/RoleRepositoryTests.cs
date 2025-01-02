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

    // Test: GetAllAsync should return all roles
    [Fact]
    public async Task GetAllAsync_ShouldReturnAllRoles()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
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

    // Test: GetByIdAsync should return correct role
    [Fact]
    public async Task GetByIdAsync_ShouldReturnRole_WhenRoleExists()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
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

    // Test: GetByIdAsync should return null for non-existing role
    [Fact]
    public async Task GetByIdAsync_ShouldReturnNull_WhenRoleDoesNotExist()
    {
        await using (var context = new ApplicationDbContext(_options))
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
        await using (var context = new ApplicationDbContext(_options))
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
        await using (var context = new ApplicationDbContext(_options))
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
        await using (var context = new ApplicationDbContext(_options))
        {
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

    // Edge Case: UpdateAsync should not update null role
    [Fact]
    public async Task UpdateAsync_ShouldThrow_WhenRoleIsNull()
    {
        await using (var context = new ApplicationDbContext(_options))
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
        await using (var context = new ApplicationDbContext(_options))
        {
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

    // Edge Case: DeleteAsync should not throw for non-existing role
    [Fact]
    public async Task DeleteAsync_ShouldNotThrow_WhenRoleDoesNotExist()
    {
        await using (var context = new ApplicationDbContext(_options))
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
        await using (var context = new ApplicationDbContext(_options))
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

    // Test: SoftDeleteAsync should mark role as deleted
    [Fact]
    public async Task SoftDeleteAsync_ShouldMarkRoleAsDeleted()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
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

    // Edge Case: SoftDeleteAsync should throw for non-existing role
    [Fact]
    public async Task SoftDeleteAsync_ShouldThrow_WhenRoleDoesNotExist()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);

            Func<Task> act = async () => await repository.SoftDeleteAsync(999);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }
    }

    // Test: GetAllAttached should return IQueryable of all roles
    [Fact]
    public async Task GetAllAttached_ShouldReturnQueryableRoles()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            context.Roles.Add(new Role { Id = 9, Name = "QueryableRole1" });
            context.Roles.Add(new Role { Id = 10, Name = "QueryableRole2" });
            await context.SaveChangesAsync();
        }

        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var rolesQuery = repository.GetAllAttached();
            var roles = rolesQuery.ToList();

            roles.Should().HaveCount(2);
            roles.Should().ContainSingle(r => r.Name == "QueryableRole1");
            roles.Should().ContainSingle(r => r.Name == "QueryableRole2");
        }
    }

    // Edge Case: GetAllAttached should return empty list when no roles exist
    [Fact]
    public async Task GetAllAttached_ShouldReturnEmptyList_WhenNoRolesExist()
    {
        await using (var context = new ApplicationDbContext(_options))
        {
            var repository = new RoleRepository(context);
            var rolesQuery = repository.GetAllAttached();
            var roles = rolesQuery.ToList();

            roles.Should().BeEmpty();
        }
    }

}
