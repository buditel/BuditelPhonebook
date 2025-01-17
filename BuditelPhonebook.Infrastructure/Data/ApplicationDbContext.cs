using BuditelPhonebook.Infrastructure.Data.Models;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace BuditelPhonebook.Infrastructure.Data
{
    public class ApplicationDbContext : DbContext
    {
        public DbSet<Person> People { get; set; }
        public DbSet<Role> Roles { get; set; }
        public DbSet<Department> Departments { get; set; }
        public DbSet<UserRole> UsersRoles { get; set; }
        public DbSet<ChangeLog> ChangeLogs { get; set; }

        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            modelBuilder.Entity<ChangeLog>()
                .Property(e => e.ChangesDescriptions)
                .HasConversion(
                    v => JsonSerializer.Serialize(v, (JsonSerializerOptions)null),
                    v => JsonSerializer.Deserialize<List<string>>(v, (JsonSerializerOptions)null));
        }
    }

}
