using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    [PrimaryKey(nameof(PersonId), nameof(SubjectId))]
    public class PersonSubject
    {
        [Required]
        [ForeignKey(nameof(Person))]
        public int PersonId { get; set; }

        public Person Person { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Subject))]
        public int SubjectId { get; set; }

        public Subject Subject { get; set; } = null!;
    }
}
