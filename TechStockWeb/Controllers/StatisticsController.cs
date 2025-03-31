using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;

namespace TechStockWeb.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly TechStockContext _context;

        public StatisticsController(TechStockContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetStatistics()
        {
            var totalProducts = await _context.Products.CountAsync();
            var assignedProducts = await _context.MaterialManagement.CountAsync();
            var unassignedProducts = totalProducts - assignedProducts;

            var data = new
            {
                totalProducts,
                assignedProducts,
                unassignedProducts
            };

            return Ok(data);
        }
    }
}
