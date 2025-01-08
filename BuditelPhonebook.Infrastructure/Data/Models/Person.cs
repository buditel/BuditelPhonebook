using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Person
    {
        public Person()
        {
            HireDate = DateTime.Now;
        }

        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string FirstName { get; set; } = null!;

        [MaxLength(50)]
        public string? MiddleName { get; set; }

        [Required, MaxLength(50)]
        public string LastName { get; set; } = null!;

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required]
        [MaxLength(20)]
        public string PersonalPhoneNumber { get; set; } = null!;

        [MaxLength(20)]
        public string? BusinessPhoneNumber { get; set; }


        [MaxLength(6)]
        public string? Birthdate { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public DateTime? LeaveDate { get; set; }

        public byte[]? PersonPicture { get; set; }

        [Required]
        [ForeignKey(nameof(Role))]
        public int RoleId { get; set; }

        [Required]
        public Role Role { get; set; } = null!;

        [MaxLength(20)]
        public string? SubjectGroup { get; set; }

        [MaxLength(40)]
        public string? Subject { get; set; }

        [Required]
        [ForeignKey(nameof(Department))]
        public int DepartmentId { get; set; }

        [Required]
        public Department Department { get; set; } = null!;

        [Required]
        public bool IsDeleted { get; set; }

        [MaxLength(150)]
        public string? CommentOnDeletion { get; set; }
    }

}
