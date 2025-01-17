using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    public class ChangeLog
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(Person))]
        [Required]
        public int PersonId { get; set; }

        [Required]
        public Person Person { get; set; } = null!;

        [Required]
        public List<string> ChangesDescriptions { get; set; }
            = new List<string>();

        [Required]
        [MaxLength(80)]
        public string ChangedBy { get; set; } = null!;

        [Required]
        public DateTime ChangedAt { get; set; }
    }
}
