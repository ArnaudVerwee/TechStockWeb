using System.ComponentModel.DataAnnotations;

namespace TechStockWeb.Models
{
    public class User
    {
        [Key]
        public int Id { get; set; }

        public required string Login { get; set; }
        
        
        public required string Password { get; set; }
       
        public DateTime CreatedAt { get; set; }
    }
}
