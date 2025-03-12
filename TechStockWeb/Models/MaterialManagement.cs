using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using TechStockWeb.Areas.Identity.Data;

namespace TechStockWeb.Models
{
    public class MaterialManagement
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey("User")]
        public string UserId { get; set; }
        public TechStockWebUser User { get; set; }

        [ForeignKey("Product")]
        public int ProductId { get; set; }
        public Product Product { get; set; }

        [ForeignKey("States")]
        public int StateId { get; set; }
        public States State { get; set; }

        public string Signature { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime SignatureDate { get; set; }
    }
}
