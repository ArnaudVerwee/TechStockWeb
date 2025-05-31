namespace TechStockWeb.Models
{
    public class CreateProductRequest
    {
        public string Name { get; set; } = string.Empty;
        public string SerialNumber { get; set; } = string.Empty;
        public int TypeId { get; set; }
        public int SupplierId { get; set; }
    }
}