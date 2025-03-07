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
        public required int TypeId { get; set; }
        public TypeArticle TypeArticle { get; set; }    


        [ForeignKey("Supplier")]
        public  required int SupplierId { get; set; }
        public Supplier Supplier { get; set; }
    }
}

