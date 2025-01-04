using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Tests.Integration
{
    public class DepartmentRepositoryIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DepartmentRepositoryIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDepartments()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 1, Name = "Finance" });
                context.Departments.Add(new Department { Id = 2, Name = "HR" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var departments = await repository.GetAllAsync();

                departments.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDepartment_WhenDepartmentExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 3, Name = "Legal" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var department = await repository.GetByIdAsync(3);

                department.Should().NotBeNull();
                department.Name.Should().Be("Legal");
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkDepartmentAsDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 4, Name = "Operations", IsDeleted = false });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                await repository.SoftDeleteAsync(4);

                var deletedDepartment = await context.Departments.FindAsync(4);
                deletedDepartment.Should().NotBeNull();
                deletedDepartment.IsDeleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDepartmentIsNull()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                Func<Task> act = async () => await repository.AddAsync(null);
                await act.Should().ThrowAsync<NullReferenceException>();
            }
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDepartmentIsNull()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                Func<Task> act = async () => await repository.UpdateAsync(null);
                await act.Should().ThrowAsync<NullReferenceException>();
            }
        }

        [Fact]
        public async Task DeleteAsync_ShouldNotThrow_WhenDepartmentDoesNotExist()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var exception = await Record.ExceptionAsync(() => repository.DeleteAsync(999));
                exception.Should().BeNull();
            }
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldThrow_WhenDepartmentDoesNotExist()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                Func<Task> act = async () => await repository.SoftDeleteAsync(999);
                await act.Should().ThrowAsync<ArgumentNullException>();
            }
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDepartmentDoesNotExist()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var department = await repository.GetByIdAsync(999);
                department.Should().BeNull();
            }
        }

        [Fact]
        public async Task GetAllAttached_ShouldReturnQueryableDepartments()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 7, Name = "Research" });
                context.Departments.Add(new Department { Id = 8, Name = "Development" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var query = repository.GetAllAttached();
                var departments = query.ToList();

                departments.Should().HaveCount(2);
            }
        }
    }
}