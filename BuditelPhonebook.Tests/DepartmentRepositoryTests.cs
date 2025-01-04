using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace BuditelPhonebook.Tests
{
    public class DepartmentRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DepartmentRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private async Task InitializeDatabase(ApplicationDbContext context)
        {
            await context.Database.EnsureDeletedAsync();
            await context.Database.EnsureCreatedAsync();

            context.Departments.AddRange(
                new Department { Id = 1, Name = "ИТ отдел" },
                new Department { Id = 2, Name = "Човешки ресурси" }
            );
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDepartments()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            var departments = await repository.GetAllAsync();

            departments.Should().HaveCount(2);
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnDepartment_WhenExists()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            var department = await repository.GetByIdAsync(1);

            department.Should().NotBeNull();
            department.Name.Should().Be("ИТ отдел");
        }

        [Fact]
        public async Task GetByIdAsync_ShouldReturnNull_WhenDepartmentDoesNotExist()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            var department = await repository.GetByIdAsync(999);

            department.Should().BeNull();
        }

        [Fact]
        public async Task AddAsync_ShouldAddDepartment()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            var department = new Department { Id = 3, Name = "Маркетинг" };

            await repository.AddAsync(department);

            var addedDepartment = await context.Departments.FindAsync(3);
            addedDepartment.Should().NotBeNull();
            addedDepartment.Name.Should().Be("Маркетинг");
        }

        [Fact]
        public async Task UpdateAsync_ShouldModifyDepartment()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            var departmentToUpdate = await repository.GetByIdAsync(1);
            departmentToUpdate.Name = "Обновен ИТ отдел";

            await repository.UpdateAsync(departmentToUpdate);

            var updatedDepartment = await context.Departments.FindAsync(1);
            updatedDepartment.Name.Should().Be("Обновен ИТ отдел");
        }

        [Fact]
        public async Task DeleteAsync_ShouldRemoveDepartment_WhenExists()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            await repository.DeleteAsync(1);

            var deletedDepartment = await context.Departments.FindAsync(1);
            deletedDepartment.Should().BeNull();
        }

        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkDepartmentAsDeleted()
        {
            await using var context = new ApplicationDbContext(_options);
            await InitializeDatabase(context);

            var repository = new DepartmentRepository(context);
            await repository.SoftDeleteAsync(1);

            var softDeletedDepartment = await context.Departments.FindAsync(1);
            softDeletedDepartment.Should().NotBeNull();
            softDeletedDepartment.IsDeleted.Should().BeTrue();
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDepartmentIsNull()
        {
            await using var context = new ApplicationDbContext(_options);
            var repository = new DepartmentRepository(context);

            Func<Task> act = async () => await repository.AddAsync(null);

            await act.Should().ThrowAsync<ArgumentNullException>();
        }

        [Fact]
        public async Task UpdateAsync_ShouldThrow_WhenDepartmentIsNull()
        {
            await using var context = new ApplicationDbContext(_options);
            var repository = new DepartmentRepository(context);

            Func<Task> act = async () => await repository.UpdateAsync(null);

            await act.Should().ThrowAsync<NullReferenceException>();
        }
    }
}
