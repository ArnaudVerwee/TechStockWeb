
using System.ComponentModel.DataAnnotations;

namespace TechStockWeb.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; }

        [Display(Name = "Username")]
        [Required]
        public string Username { get; set; }

        [Display(Name = "Email")]
        [EmailAddress]
        public string? Email { get; set; }

        
        public virtual ICollection<Product> AssignedProducts { get; set; } = new List<Product>();
    }
}