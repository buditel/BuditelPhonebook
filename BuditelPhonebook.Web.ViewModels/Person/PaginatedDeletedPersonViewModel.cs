namespace BuditelPhonebook.Web.ViewModels.Person
{
    public class PaginatedDeletedPersonViewModel
    {
        public IEnumerable<DeletedIndexPersonViewModel> People { get; set; }
            = new List<DeletedIndexPersonViewModel>();
        public int CurrentPage { get; set; }
        public int PageSize { get; set; }
        public int TotalCount { get; set; }
        public int TotalPages => (int)Math.Ceiling((double)TotalCount / PageSize);
    }
}
