using BuditelPhonebook.Core.Repositories;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.Controllers;
using BuditelPhonebook.Web.ViewModels.Role;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Tests.Integration
{
    public class RoleControllerIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public RoleControllerIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        private async Task SeedRoles(ApplicationDbContext context)
        {
            context.Roles.AddRange(
                new Role { Id = 1, Name = "Admin" },
                new Role { Id = 2, Name = "User" }
            );
            await context.SaveChangesAsync();
        }

        [Fact]
        public async Task Index_ShouldReturnViewWithAllRoles()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                await SeedRoles(context);
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);

                var result = await controller.Index() as ViewResult;

                result.Should().NotBeNull();
                var roles = result.Model as IEnumerable<Role>;
                roles.Should().HaveCount(2);
            }
        }

        [Fact]
        public void Create_ShouldReturnViewWithModel()
        {
            var controller = new RoleController(null);

            var result = controller.Create() as ViewResult;

            result.Should().NotBeNull();
            result.Model.Should().BeOfType<CreateRoleViewModel>();
        }

        [Fact]
        public async Task Create_ShouldAddRole_WhenModelIsValid()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);
                var model = new CreateRoleViewModel { Name = "Manager" };

                var result = await controller.Create(model) as RedirectToActionResult;

                result.ActionName.Should().Be(nameof(RoleController.Index));

                var addedRole = await context.Roles.FirstOrDefaultAsync(r => r.Name == "Manager");
                addedRole.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Create_ShouldReturnViewWithError_WhenRoleNameExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                await SeedRoles(context);
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);
                var model = new CreateRoleViewModel { Name = "Admin" };

                var result = await controller.Create(model) as ViewResult;

                result.Should().NotBeNull();
                result.ViewData.ModelState.IsValid.Should().BeFalse();
                result.ViewData.ModelState[nameof(model.Name)].Errors.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Edit_ShouldReturnViewWithRole_WhenRoleExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 3, Name = "Operator" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);

                var result = await controller.Edit(3) as ViewResult;

                result.Should().NotBeNull();
                var model = result.Model as EditRoleViewModel;
                model.Name.Should().Be("Operator");
            }
        }

        [Fact]
        public async Task Edit_ShouldReturnNotFound_WhenRoleDoesNotExist()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);

                var result = await controller.Edit(999) as NotFoundResult;

                result.Should().NotBeNull();
            }
        }

        [Fact]
        public async Task Edit_ShouldUpdateRole_WhenModelIsValid()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 4, Name = "OldRole" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);
                var model = new EditRoleViewModel { Id = 4, Name = "UpdatedRole" };

                var result = await controller.Edit(model) as RedirectToActionResult;

                result.ActionName.Should().Be(nameof(RoleController.Index));

                var updatedRole = await context.Roles.FindAsync(4);
                updatedRole.Name.Should().Be("UpdatedRole");
            }
        }

        [Fact]
        public async Task Edit_ShouldReturnError_WhenNameAlreadyExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                await SeedRoles(context);
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);
                var model = new EditRoleViewModel { Id = 1, Name = "User" };

                var result = await controller.Edit(model) as ViewResult;

                result.Should().NotBeNull();
                result.ViewData.ModelState.IsValid.Should().BeFalse();
                result.ViewData.ModelState[nameof(model.Name)].Errors.Should().NotBeEmpty();
            }
        }

        [Fact]
        public async Task Delete_ShouldReturnViewWithRole_WhenRoleExists()
        {
            await using (var context = new ApplicationDbContext(_options))
            {
                context.Roles.Add(new Role { Id = 5, Name = "ToDelete" });
                await context.SaveChangesAsync();
            }

            await using (var context = new ApplicationDbContext(_options))
            {
                var repository = new RoleRepository(context);
                var controller = new RoleController(repository);

                var result = await controller.Delete(5) as ViewResult;

                result.Should().NotBeNull();
                var role = result.Model as Role;
                role.Name.Should().Be("ToDelete");
            }
        }
    }
}
