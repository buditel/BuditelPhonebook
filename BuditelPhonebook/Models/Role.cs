using System.ComponentModel.DataAnnotations;

namespace BuditelPhonebook.Models
{
    public class Role
    {
        [Key]
        public int Id { get; set; }

        public string Name { get; set; }
    }
}
