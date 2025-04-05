using System.ComponentModel.DataAnnotations;

using static BuditelPhonebook.Common.EntityValidationMessages.Subject;

namespace BuditelPhonebook.Web.ViewModels.Subject
{
    public class CreateSubjectViewModel
    {
        [Required(ErrorMessage = NameRequiredMessage)]
        [StringLength(100, MinimumLength = 2, ErrorMessage = NameLengthMessage)]
        public string Name { get; set; } = null!;
    }
}
