using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string Name { get; set; } = null!;

        [Required]
        public bool IsDeleted { get; set; }
    }
}
