using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Models
{
    public class Person
    {
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string? FirstName { get; set; } 

        [MaxLength(50)]
        public string? MiddleName { get; set; } 

        [Required, MaxLength(50)]
        public string? LastName { get; set; }

        [Required, MaxLength(100)]
        public string? Email { get; set; }

        [MaxLength(20)]
        public string? PhoneNumber { get; set; }

        public int RoleId { get; set; }
        public Role Role { get; set; }

        public int DepartmentId { get; set; }
        public Department Department { get; set; }
    }

}
