using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    [Index(nameof(Email), IsUnique = true)]
    public class Person
    {
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
        [MaxLength(10)]
        public string PersonalPhoneNumber { get; set; } = null!;

        [MaxLength(10)]
        public string? BusinessPhoneNumber { get; set; }


        [MaxLength(6)]
        public string? Birthdate { get; set; }

        [Required]
        public DateTime HireDate { get; set; }

        public DateTime? LeaveDate { get; set; }

        public byte[]? PersonPicture { get; set; }

        public ICollection<PersonRole> PeopleRoles { get; set; }
            = new List<PersonRole>();

        [MaxLength(20)]
        public string? SubjectGroup { get; set; }

        public ICollection<PersonSubject> PeopleSubjects { get; set; }
            = new List<PersonSubject>();

        public ICollection<PersonDepartment> PeopleDepartments { get; set; }
            = new List<PersonDepartment>();

        [Required]
        public bool IsDeleted { get; set; }

        [MaxLength(150)]
        public string? CommentOnDeletion { get; set; }

        public ICollection<ChangeLog> ChangeLogs { get; set; }
            = new List<ChangeLog>();
    }

}
