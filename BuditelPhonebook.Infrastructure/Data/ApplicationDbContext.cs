using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;

namespace BuditelPhonebook.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }
    }

}
