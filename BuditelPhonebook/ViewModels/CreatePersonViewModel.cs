using BuditelPhonebook.Models;
using System.ComponentModel.DataAnnotations;
using static BuditelPhonebook.Common.EntityValidationConstants.Person;
using static BuditelPhonebook.Common.EntityValidationMessages.Person;

namespace BuditelPhonebook.ViewModels
{
    public class CreatePersonViewModel : IValidatableObject
    {
        [Required(ErrorMessage = FirstNameRequiredMessage), StringLength(50, MinimumLength = 2, ErrorMessage = FirstNameLengthMessage)]
        public string FirstName { get; set; } = null!;

        [StringLength(50, MinimumLength = 2, ErrorMessage = MiddleNameLengthMessage)]
        public string? MiddleName { get; set; }

        [Required(ErrorMessage = LastNameRequiredMessage), StringLength(50, MinimumLength = 2, ErrorMessage = LastNameLengthMessage)]
        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = EmailRequiredMessage), StringLength(100, MinimumLength = 7, ErrorMessage = EmailLengthMessage)]
        public string Email { get; set; } = null!;

        [StringLength(20, MinimumLength = 7, ErrorMessage = BusinessPhoneNumberLengthMessage)]
        public string? BusinessPhoneNumber { get; set; }

        [StringLength(20, MinimumLength = 7, ErrorMessage = PersonalPhoneNumberLengthMessage)]
        public string? PersonalPhoneNumber { get; set; }

        [RegularExpression(PersonBirthDateRegexPattern, ErrorMessage = BirthDateWrongFormatMessage)]
        public string? Birthdate { get; set; }

        [Required(ErrorMessage = RoleRequiredMessage)]
        public string Role { get; set; } = null!;

        public IEnumerable<Role> Roles { get; set; }
            = new List<Role>();

        [Required(ErrorMessage = SubjectGroupRequiredMessage)]
        [MaxLength(20)]
        public string? SubjectGroup { get; set; }

        [Required(ErrorMessage = SubjectRequiredMessage)]
        [MaxLength(40)]
        public string? Subject { get; set; }

        [Required(ErrorMessage = DepartmentRequiredMessage)]
        public string Department { get; set; } = null!;

        public IEnumerable<Department> Departments { get; set; }
            = new List<Department>();

        public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
        {
            if (Role == "Учител")
            {
                if (string.IsNullOrWhiteSpace(SubjectGroup))
                {
                    yield return new ValidationResult(SubjectGroupRequiredMessage, new[] { "SubjectGroup" });
                }

                if (string.IsNullOrWhiteSpace(Subject))
                {
                    yield return new ValidationResult(SubjectRequiredMessage, new[] { "Subject" });
                }
            }
        }
    }
}
