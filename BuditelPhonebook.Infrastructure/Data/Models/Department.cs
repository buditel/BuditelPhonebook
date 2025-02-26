using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    [Index(nameof(Name), IsUnique = true)]
    public class Department
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public bool IsDeleted { get; set; }

        public IList<Person> People { get; set; }
            = new List<Person>();
    }
}
