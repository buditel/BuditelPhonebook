using System.ComponentModel.DataAnnotations;

using static BuditelPhonebook.Common.EntityValidationMessages.Department;

namespace BuditelPhonebook.Web.ViewModels.Department
{
    public class EditDepartmentViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = NameRequiredMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = NameLengthMessage)]
        public string Name { get; set; } = null!;
    }
}
