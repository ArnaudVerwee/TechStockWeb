using System.ComponentModel.DataAnnotations;

namespace TechStockWeb.Models
{
    public class States
    {
        [Key]
        public int Id { get; set; }
        public string Status { get; set; }
    }
}
