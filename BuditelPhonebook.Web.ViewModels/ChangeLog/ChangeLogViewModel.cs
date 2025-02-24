namespace BuditelPhonebook.Web.ViewModels.ChangeLog
{
    public class ChangeLogViewModel
    {
        public List<string> ChangesDescriptions { get; set; }
            = new List<string>();

        public string ChangedBy { get; set; } = null!;

        public string ChangedAt { get; set; } = null!;
    }
}
