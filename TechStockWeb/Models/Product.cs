using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechStockWeb.Models
{
    public class Product
    {
        [Key]
        public int Id { get; set; }
        public required string Name { get; set; }
        public required string SerialNumber { get; set; }
        [ForeignKey("TypeArticle")]
        public required string TypeId { get; set; }
        [ForeignKey("Supplier")]
        public  required string SupplierId { get; set; }
    }
}

