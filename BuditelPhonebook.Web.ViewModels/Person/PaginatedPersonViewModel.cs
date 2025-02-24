namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class PaginatedPersonViewModel
    {
        public IEnumerable<PersonDetailsViewModel> People { get; set; }
            = new List<PersonDetailsViewModel>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
