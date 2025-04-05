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

        public string? BusinessPhoneNumber { get; set; }

        public string? Birthdate { get; set; }

        public string? LeaveDate { get; set; }

        public string? SubjectGroup { get; set; }

        public List<string> Subjects { get; set; }
                    = new List<string>();

        public byte[]? PersonPicture { get; set; }

        public string HireDate { get; set; } = null!;

        public List<string> Roles { get; set; }
                    = new List<string>();
        public List<string> Departments { get; set; }
            = new List<string>();
    }
}
