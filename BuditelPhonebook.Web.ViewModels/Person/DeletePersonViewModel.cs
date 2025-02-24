namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class DeletePersonViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;

        public string? CommentOnDeletion { get; set; }

        public string LeaveDate { get; set; } = null!;
    }
}
