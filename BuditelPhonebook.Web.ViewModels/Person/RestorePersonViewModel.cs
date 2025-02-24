namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class RestorePersonViewModel
    {
        public int Id { get; set; }

        public string FirstName { get; set; } = null!;

        public string? MiddleName { get; set; }

        public string LastName { get; set; } = null!;
    }
}
