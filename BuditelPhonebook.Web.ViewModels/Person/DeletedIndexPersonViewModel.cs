namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class DeletedIndexPersonViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string? CommentOnDeletion { get; set; }

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? PersonalPhoneNumber { get; set; }

        public string? LeaveDate { get; set; }

        public string Role { get; set; } = null!;

        public string Department { get; set; } = null!;
    }
}
