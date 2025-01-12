using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Infrastructure.Data.Models
{
    public class UserRole
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string Email { get; set; } = null!;

        [Required, MaxLength(50)]
        public string Role { get; set; } = null!;
    }
}
