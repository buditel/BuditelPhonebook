using BuditelPhonebook.Common.CustomAttributes;
using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

using static BuditelPhonebook.Common.EntityValidationConstants.Person;
using static BuditelPhonebook.Common.EntityValidationMessages.Person;

namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class EditPersonViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = FirstNameRequiredMessage), StringLength(50, MinimumLength = 2, ErrorMessage = FirstNameLengthMessage)]
        public string FirstName { get; set; } = null!;

        [StringLength(50, MinimumLength = 2, ErrorMessage = MiddleNameLengthMessage)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = LastNameRequiredMessage), StringLength(50, MinimumLength = 2, ErrorMessage = LastNameLengthMessage)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = EmailRequiredMessage), StringLength(100, MinimumLength = 7, ErrorMessage = EmailLengthMessage)]
        [RegularExpression(EmailRegexPattern, ErrorMessage = EmailWrongFormatMessage)]
        public string Email { get; set; } = null!;


        [Required(ErrorMessage = PersonalPhoneNumberRequiredMessage)]
        [StringLength(20, MinimumLength = 7, ErrorMessage = PersonalPhoneNumberLengthMessage)]
        public string PersonalPhoneNumber { get; set; } = null!;

        [StringLength(20, MinimumLength = 7, ErrorMessage = BusinessPhoneNumberLengthMessage)]
        public string? BusinessPhoneNumber { get; set; }

        [RegularExpression(BirthDateRegexPattern, ErrorMessage = BirthDateWrongFormatMessage)]
        public string? Birthdate { get; set; }

        public byte[]? ExistingPicture { get; set; }

        public IFormFile? PersonPicture { get; set; }

        [Required(ErrorMessage = RoleRequiredMessage)]
        public string Role { get; set; } = null!;

        public IEnumerable<Infrastructure.Data.Models.Role> Roles { get; set; }
            = new List<Infrastructure.Data.Models.Role>();

        [MaxLength(20)]
        [RequiredIfTeacher("Role", "Учител", SubjectGroupRequiredMessage)]
        public string? SubjectGroup { get; set; }

        [MaxLength(40)]
        [RequiredIfTeacher("Role", "Учител", SubjectRequiredMessage)]
        public string? Subject { get; set; }

        [Required(ErrorMessage = DepartmentRequiredMessage)]
        public string Department { get; set; } = null!;

        public IEnumerable<Infrastructure.Data.Models.Department> Departments { get; set; }
            = new List<Infrastructure.Data.Models.Department>();
    }
}
