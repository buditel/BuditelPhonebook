using System.ComponentModel.DataAnnotations;
using static BuditelPhonebook.Common.EntityValidationConstants.UserRole;
using static BuditelPhonebook.Common.EntityValidationMessages.UserRole;

namespace BuditelPhonebook.Web.ViewModels.UserRole
{
    public class UserRoleViewModel
    {
        [Required(ErrorMessage = EmailRequiredMessage), StringLength(100, MinimumLength = 7, ErrorMessage = EmailLengthMessage)]
        [RegularExpression(EmailRegexPattern, ErrorMessage = EmailWrongFormatMessage)]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = UserRoleRequiredMessage)]
        [RegularExpression(UserRoleRegexPattern, ErrorMessage = RoleWrongFormatMessage)]
        public string Role { get; set; } = null!;

        public IEnumerable<Infrastructure.Data.Models.UserRole> UserRoles { get; set; }
            = new List<Infrastructure.Data.Models.UserRole>();
    }
}
