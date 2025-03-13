using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    [PrimaryKey(nameof(PersonId), nameof(DepartmentId))]
    public class PersonDepartment
    {
        [Required]
        [ForeignKey(nameof(Person))]
        public int PersonId { get; set; }

        public Person Person { get; set; } = null!;

        [Required]
        [ForeignKey(nameof(Department))]
        public int DepartmentId { get; set; }

        public Department Department { get; set; } = null!;
    }
}
