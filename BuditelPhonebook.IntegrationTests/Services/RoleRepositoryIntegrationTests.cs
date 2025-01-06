using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.IntegrationTests.Services
{
    public class RoleRepositoryIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public RoleRepositoryIntegrationTests()
        {
            // Създаване на InMemory база за всеки тест
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllRoles()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                // Arrange - добавяне на тестови данни
                context.Roles.Add(new Role { Id = 1, Name = "Admin" });
                context.Roles.Add(new Role { Id = 2, Name = "User" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                // Act
                var repository = new RoleRepository(context);
                var roles = await repository.GetAllAsync();

                // Assert
                roles.Should().HaveCount(2);
                roles.Should().Contain(r => r.Name == "Admin");
                roles.Should().Contain(r => r.Name == "User");
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnRole_WhenRoleExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 3, Name = "Manager" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var role = await repository.GetByIdAsync(3);

                role.Should().NotBeNull();
                role.Name.Should().Be("Manager");
            }
        }

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

        [Fact]
        public async Task AddAsync_ShouldAddRole()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var role = new Role { Name = "NewRole" };

                await repository.AddAsync(role);

                var addedRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "NewRole");
                addedRole.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyRole()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 4, Name = "OldRole" };
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var roleToUpdate = await repository.GetByIdAsync(4);
                roleToUpdate.Name = "UpdatedRole";

                await repository.UpdateAsync(roleToUpdate);

                var updatedRole = await context.Roles.FindAsync(4);
                updatedRole.Name.Should().Be("UpdatedRole");
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveRole_WhenRoleExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 5, Name = "RoleToDelete" });
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
        public async Task SoftDeleteAsync_ShouldMarkRoleAsDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 6, Name = "SoftDeleteRole" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                await repository.SoftDeleteAsync(6);

                var softDeletedRole = await context.Roles.FindAsync(6);
                softDeletedRole.Should().NotBeNull();
                softDeletedRole.IsDeleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldNotThrow_WhenRoleAlreadyDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 1, Name = "AlreadyDeletedRole", IsDeleted = true };
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                await repository.SoftDeleteAsync(1);

                var softDeletedRole = await context.Roles.FindAsync(1);
                softDeletedRole.Should().NotBeNull();
                softDeletedRole.IsDeleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenRoleNameIsDuplicate()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Name = "DuplicateRole" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var duplicateRole = new Role { Name = "DuplicateRole" };

                Func<Task> act = async () => await repository.AddAsync(duplicateRole);
                await act.Should().ThrowAsync<DbUpdateException>();
            }
        }

        [Fact]
        public async Task GetAllAttached_ShouldReturnOnlyNotDeletedRoles()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 1, Name = "ActiveRole", IsDeleted = false });
                context.Roles.Add(new Role { Id = 2, Name = "DeletedRole", IsDeleted = true });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var roles = repository.GetAllAttached().Where(r => !r.IsDeleted).ToList();

                roles.Should().HaveCount(1);
                roles.First().Name.Should().Be("ActiveRole");
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldHandleConcurrentUpdates()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var role = new Role { Id = 6, Name = "ConcurrentRole" };
                context.Roles.Add(role);
                await context.SaveChangesAsync();
            }

            await using (var context1 = new ApplicationDbContext(_options))
            await using (var context2 = new ApplicationDbContext(_options))
            {
                var repository1 = new RoleRepository(context1);
                var repository2 = new RoleRepository(context2);

                var role1 = await repository1.GetByIdAsync(6);
                var role2 = await repository2.GetByIdAsync(6);

                role1.Name = "UpdateFromContext1";
                role2.Name = "UpdateFromContext2";

                await repository1.UpdateAsync(role1);

                Func<Task> act = async () => await repository2.UpdateAsync(role2);
                await act.Should().ThrowAsync<InvalidOperationException>()
                    .WithMessage("The role was updated by another process.");
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var updatedRole = await context.Roles.FindAsync(6);
                updatedRole.Name.Should().Be("UpdateFromContext1");
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldHandleConcurrentDeletes()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 10, Name = "TemporaryRole" });
                await context.SaveChangesAsync();
            }

            await using (var context1 = new ApplicationDbContext(_options))
            await using (var context2 = new ApplicationDbContext(_options))
            {
                var repository1 = new RoleRepository(context1);
                var repository2 = new RoleRepository(context2);

                var task1 = repository1.DeleteAsync(10);
                var task2 = repository2.DeleteAsync(10);

                await Task.WhenAll(task1, task2);

                var deletedRole = await context1.Roles.FindAsync(10);
                deletedRole.Should().BeNull();
            }
        }

    }
}
