using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace TechStockWeb.Models
{
    public class MaterialManagement
    {
        [Key]
        public int Id { get; set; }
        [ForeignKey("User")]
        public int UserId { get; set; }
        [ForeignKey("Product")]
        public int ProductId { get; set; }
        [ForeignKey("States")]
        public int StateId { get; set; }
        public string Signature { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime SignatureDate { get; set; }
    }
}
