using System.ComponentModel.DataAnnotations;

using static BuditelPhonebook.Common.EntityValidationMessages.Role;

namespace BuditelPhonebook.Web.ViewModels.Role
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = NameRequiredMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = NameLengthMessage)]
        public string Name { get; set; } = null!;
    }
}
