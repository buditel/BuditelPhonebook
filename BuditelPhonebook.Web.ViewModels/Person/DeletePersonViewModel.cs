using System.ComponentModel.DataAnnotations;

using static BuditelPhonebook.Common.EntityValidationMessages.Person;

namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class DeletePersonViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;

        [Required(ErrorMessage = CommentOnDeletionRequiredMessage), StringLength(150, MinimumLength = 5, ErrorMessage = CommentOnDeletionLengthMessage)]
        public string CommentOnDeletion { get; set; } = null!;

        public string LeaveDate { get; set; } = null!;
    }
}
