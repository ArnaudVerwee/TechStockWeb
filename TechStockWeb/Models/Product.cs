using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechStockWeb.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Display(Name = "Name")]
        [Required]
        public string Name { get; set; } = string.Empty;

        [Display(Name = "SerialNumber")]
        [Required]
        public string SerialNumber { get; set; } = string.Empty;

        [Display(Name = "Item Types")]
        [ForeignKey("TypeArticle")]
        [Required]
        public int TypeId { get; set; }

        // IMPORTANT: Enlever [Required] sur les propriétés de navigation
        public TypeArticle? TypeArticle { get; set; }

        [Display(Name = "Supplier")]
        [ForeignKey("Supplier")]
        [Required]
        public int SupplierId { get; set; }

        // IMPORTANT: Enlever [Required] sur les propriétés de navigation
        public Supplier? Supplier { get; set; }

        [Display(Name = "Assigned User")]
        public int? AssignedUserId { get; set; }

        [ForeignKey("AssignedUserId")]
        public User? AssignedUser { get; set; }
    }
}