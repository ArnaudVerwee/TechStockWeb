using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechStockWeb.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }

        [Display(Name ="Name")]
        public required string Name { get; set; }

        [Display(Name="SerialNumber")]
        public required string SerialNumber { get; set; }

        [Display(Name = "Item Types")]
        [ForeignKey("TypeArticle")]
        public required int TypeId { get; set; }
        public TypeArticle TypeArticle { get; set; }

        [Display(Name = "Supplier")]
        [ForeignKey("Supplier")]
        public required int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
    }
}

