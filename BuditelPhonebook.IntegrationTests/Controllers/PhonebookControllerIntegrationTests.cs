using BuditelPhonebook.Core.Contracts;
using BuditelPhonebook.Infrastructure.Data;
using BuditelPhonebook.Infrastructure.Data.Models;
using BuditelPhonebook.Web.Controllers;
using BuditelPhonebook.Web.ViewModels.Person;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace BuditelPhonebook.Tests.Integration
{
    public class PhonebookControllerIntegrationTests
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;
        private readonly Mock<IPersonRepository> _mockPersonRepository;
        private readonly PhonebookController _controller;

        public PhonebookControllerIntegrationTests()
        {
            _options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString()) // Уникална база за всеки тест
                .Options;

            _mockPersonRepository = new Mock<IPersonRepository>();
            _controller = new PhonebookController(_mockPersonRepository.Object);
        }

        [Fact]
        public async Task Details_ShouldReturnViewWithPersonDetails_WhenPersonExists()
        {
            // Arrange
            var person = new Person
            {
                Id = 1,
                FirstName = "Иван",
                LastName = "Петров",
                Email = "ivan.petrov@example.com",
                PersonalPhoneNumber = "0888123456",
                BusinessPhoneNumber = "024567890",
                Birthdate = "01.10.",
                RoleId = 1,
                DepartmentId = 1,
                Role = new Role { Name = "Учител" },
                Department = new Department { Name = "ИТ отдел" }
            };

            _mockPersonRepository.Setup(repo => repo.GetByIdWithRelationsAsync(1))
                .ReturnsAsync(person);

            // Act
            var result = await _controller.Details(1) as ViewResult;

            // Assert
            result.Should().NotBeNull();
            result.Model.Should().BeAssignableTo<PersonDetailsViewModel>();
            var model = result.Model as PersonDetailsViewModel;
            model.FirstName.Should().Be("Иван");
            model.Role.Should().Be("Учител");
            model.Email.Should().Be("ivan.petrov@example.com");
        }

        [Fact]
        public async Task Details_ShouldThrowException_WhenPersonDoesNotExist()
        {
            // Arrange
            _mockPersonRepository.Setup(repo => repo.GetByIdWithRelationsAsync(999))
                .ReturnsAsync((Person)null);

            // Act
            Func<Task> act = async () => await _controller.Details(999);

            // Assert
            await act.Should().ThrowAsync<Exception>();
        }

        //[Fact]
        //public async Task Index_ShouldReturnPartialView_ForAjaxRequest()
        //{
        //    // Arrange
        //    var people = new List<PersonDetailsViewModel>
        //    {
        //        new PersonDetailsViewModel
        //        {
        //            Id = 1,
        //            FirstName = "Иван",
        //            LastName = "Петров",
        //            Email = "ivan.petrov@example.com",
        //            PersonalPhoneNumber = "0888123456",
        //            Role = "Учител",
        //            Department = "ИТ отдел"
        //        }
        //    };

        //    _mockPersonRepository.Setup(repo => repo.SearchAsync("Иван"))
        //        .ReturnsAsync(people);

        //    _controller.ControllerContext.HttpContext = new DefaultHttpContext();
        //    _controller.ControllerContext.HttpContext.Request.Headers["X-Requested-With"] = "XMLHttpRequest";

        //    // Act
        //    var result = await _controller.Index("Иван") as PartialViewResult;

        //    // Assert
        //    result.Should().NotBeNull();
        //    result.ViewName.Should().Be("_PeopleTablePartial");
        //    result.Model.Should().BeAssignableTo<IEnumerable<PersonDetailsViewModel>>();
        //}
    }
}
