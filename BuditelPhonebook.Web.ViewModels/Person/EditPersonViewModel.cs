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
        [RegularExpression(PhoneNumberRegexPattern, ErrorMessage = PersonalPhoneNumberLengthMessage)]
        public string PersonalPhoneNumber { get; set; } = null!;

        [RegularExpression(PhoneNumberRegexPattern, ErrorMessage = BusinessPhoneNumberLengthMessage)]
        public string? BusinessPhoneNumber { get; set; }

        [RegularExpression(BirthDateRegexPattern, ErrorMessage = BirthDateWrongFormatMessage)]
        public string? Birthdate { get; set; }

        [Required(ErrorMessage = HireDateRequiredMessage)]
        [RegularExpression(HireAndLeaveDateRegexPattern, ErrorMessage = HireDateWrongFormatMessage)]
        public string HireDate { get; set; } = null!;

        public byte[]? ExistingPicture { get; set; }

        public IFormFile? PersonPicture { get; set; }

        [Required(ErrorMessage = RoleRequiredMessage)]
        public List<string> Roles { get; set; }
             = new List<string>();

        public IEnumerable<Infrastructure.Data.Models.Role> AvailableRoles { get; set; }
            = new List<Infrastructure.Data.Models.Role>();

        [MaxLength(20)]
        [RequiredIfTeacher("Roles", "Учител", SubjectGroupRequiredMessage)]
        public string? SubjectGroup { get; set; }

        [RequiredIfTeacher("Roles", "Учител", SubjectRequiredMessage)]
        public List<string> Subjects { get; set; }
            = new List<string>();

        public IEnumerable<Infrastructure.Data.Models.Subject> AvailableSubjects { get; set; }
            = new List<Infrastructure.Data.Models.Subject>();

        [Required(ErrorMessage = DepartmentRequiredMessage)]
        public List<string> Departments { get; set; }
            = new List<string>();

        public IEnumerable<Infrastructure.Data.Models.Department> AvailableDepartments { get; set; }
            = new List<Infrastructure.Data.Models.Department>();
    }
}
