using System.ComponentModel.DataAnnotations;

namespace TechStockWeb.Models
{
    public class TypeArticle
    {
        [Key]
        public int Id { get; set; }
        public string Type { get; set; }
    }
}
