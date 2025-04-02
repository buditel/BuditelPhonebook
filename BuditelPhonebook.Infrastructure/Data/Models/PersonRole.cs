using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    [PrimaryKey(nameof(PersonId), nameof(RoleId))]
    public class PersonRole
    {
        [Required]
        [ForeignKey(nameof(Person))]
        public int PersonId { get; set; }

        public Person Person { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        public Role Role { get; set; } = null!;
    }
}
