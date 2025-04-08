using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;
using System.Threading.Tasks;

namespace TechStockWeb.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StatisticsController : ControllerBase
    {
        private readonly TechStockContext _context;

        public StatisticsController(TechStockContext context)
        {
            _context = context;
        }

        // GET: api/statistics
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
