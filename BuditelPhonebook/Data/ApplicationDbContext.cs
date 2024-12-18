using BuditelPhonebook.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        public DbSet<Person> People { get; set; }
    }

}
