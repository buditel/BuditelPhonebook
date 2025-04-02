namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class PersonDetailsViewModel
    {
        public int Id { get; set; }
        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;

        public string Email { get; set; } = null!;

        public string? BusinessPhoneNumber { get; set; }

        public string? PersonalPhoneNumber { get; set; }

        public string? Birthdate { get; set; }

        public string HireDate { get; set; } = null!;

        public byte[]? PersonPicture { get; set; }

        public List<string> Roles { get; set; }
                    = new List<string>();
        public string? SubjectGroup { get; set; }

        public string? Subject { get; set; }

        public List<string> Departments { get; set; }
            = new List<string>();

        public string ChangedAt { get; set; } = null!;
    }
}
