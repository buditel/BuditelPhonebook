using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Tests
{
    public class DepartmentRepositoryTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public DepartmentRepositoryTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: "TestDb")
                .Options;
        }

        [Fact]
        public async Task GetAllAsync_ShouldReturnAllDepartments()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.RemoveRange(context.Departments);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 1, Name = "ИТ отдел" });
                context.Departments.Add(new Department { Id = 2, Name = "Човешки ресурси" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var departments = await repository.GetAllAsync();

                departments.Should().HaveCount(2);
            }
        }


        // Test: GetByIdAsync should return correct department
        [Fact]
        public async Task GetByIdAsync_ShouldReturnDepartment_WhenExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 3, Name = "Финанси" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var department = await repository.GetByIdAsync(3);

                department.Should().NotBeNull();
                department.Name.Should().Be("Финанси");
            }
        }

        // Test: GetByIdAsync should return null if department does not exist
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

        // Test: AddAsync should add a department
        [Fact]
        public async Task AddAsync_ShouldAddDepartment()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var department = new Department { Id = 4, Name = "Маркетинг" };

                await repository.AddAsync(department);

                var addedDepartment = await context.Departments.FindAsync(4);
                addedDepartment.Should().NotBeNull();
                addedDepartment.Name.Should().Be("Маркетинг");
            }
        }

        // Test: UpdateAsync should modify an existing department
        [Fact]
        public async Task UpdateAsync_ShouldModifyDepartment()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var department = new Department { Id = 5, Name = "Обслужване на клиенти" };
                context.Departments.Add(department);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var departmentToUpdate = await repository.GetByIdAsync(5);
                departmentToUpdate.Name = "Поддръжка";
                await repository.UpdateAsync(departmentToUpdate);

                var updatedDepartment = await context.Departments.FindAsync(5);
                updatedDepartment.Name.Should().Be("Поддръжка");
            }
        }

        // Test: DeleteAsync should remove a department
        [Fact]
        public async Task DeleteAsync_ShouldRemoveDepartment_WhenExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 6, Name = "Закупуване" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                await repository.DeleteAsync(6);

                var deletedDepartment = await context.Departments.FindAsync(6);
                deletedDepartment.Should().BeNull();
            }
        }

        // Test: SoftDeleteAsync should mark department as deleted
        [Fact]
        public async Task SoftDeleteAsync_ShouldMarkDepartmentAsDeleted()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var department = new Department { Id = 7, Name = "Продажби", IsDeleted = false };
                context.Departments.Add(department);
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                await repository.SoftDeleteAsync(7);

                var softDeletedDepartment = await context.Departments.FindAsync(7);
                softDeletedDepartment.Should().NotBeNull();
                softDeletedDepartment.IsDeleted.Should().BeTrue();
            }
        }

        [Fact]
        public async Task AddAsync_ShouldThrow_WhenDepartmentIsNull()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);

                context.Departments.Add(new Department { Id = 1, Name = "ИТ отдел" });
                await context.SaveChangesAsync();

                Func<Task> act = async () => await repository.AddAsync(null);

                await act.Should().ThrowAsync<ArgumentNullException>();
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
        public async Task GetAllAsync_ShouldReturnOnlyNotDeletedDepartments()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 1, Name = "Активен", IsDeleted = false });
                context.Departments.Add(new Department { Id = 2, Name = "Изтрит", IsDeleted = true });
                context.Departments.Add(new Department { Id = 10, Name = "Конкурентност 1", IsDeleted = false });
                context.Departments.Add(new Department { Id = 11, Name = "Конкурентност 2", IsDeleted = false });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var departments = await repository.GetAllAsync();

                var notDeletedDepartments = departments.Where(d => !d.IsDeleted);

                notDeletedDepartments.Should().HaveCount(3);
            }
        }

        [Fact]
        public async Task AddAsync_ShouldHandleConcurrentRequests()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);

                var task1 = repository.AddAsync(new Department { Id = 10, Name = "Конкурентност 1" });
                var task2 = repository.AddAsync(new Department { Id = 11, Name = "Конкурентност 2" });

                await Task.WhenAll(task1, task2);

                var departments = await context.Departments.ToListAsync();
                departments.Should().HaveCount(2);
            }
        }

        [Fact]
        public async Task GetAllAttached_ShouldReturnQueryable()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Departments.Add(new Department { Id = 10, Name = "Конкурентност 1", IsDeleted = false });
                context.Departments.Add(new Department { Id = 12, Name = "Запитване 1", IsDeleted = false });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new DepartmentRepository(context);
                var query = repository.GetAllAttached();

                var notDeletedDepartments = query.Where(d => !d.IsDeleted).ToList();

                notDeletedDepartments.Should().HaveCount(2);
                notDeletedDepartments.Should().ContainSingle(d => d.Name == "Конкурентност 1");
                notDeletedDepartments.Should().ContainSingle(d => d.Name == "Запитване 1");
            }
        }


    }
}
