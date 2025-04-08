using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Areas.Identity.Data;
using TechStockWeb.Data;
using TechStockWeb.Models;

namespace TechStockWeb.Controllers
{
    //[Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class MaterialManagementController : ControllerBase
    {
        private readonly TechStockContext _context;
        private readonly UserManager<TechStockWebUser> _userManager;

        public MaterialManagementController(TechStockContext context, UserManager<TechStockWebUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: api/MaterialManagements
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var result = await _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Include(m => m.User)
                .ToListAsync();

            return Ok(result);
        }

        // GET: api/MaterialManagements/User
        [HttpGet("User")]
        public async Task<IActionResult> GetMyAssignments()
        {
            var userId = _userManager.GetUserId(User);
            var assignments = await _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            return Ok(assignments);
        }

        // GET: api/MaterialManagements/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            var assignment = await _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assignment == null) return NotFound();

            return Ok(assignment);
        }

        // POST: api/MaterialManagements/Sign
        [HttpPost("Sign")]
        public async Task<IActionResult> SignProduct([FromBody] SignatureDto dto)
        {
            var assignment = await _context.MaterialManagement.FindAsync(dto.Id);
            if (assignment == null || assignment.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            assignment.Signature = dto.Signature;
            assignment.SignatureDate = DateTime.Now;

            _context.Update(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Signature saved." });
        }

        // POST: api/MaterialManagements/Assign
        [HttpPost("Assign")]
        public async Task<IActionResult> AssignProduct([FromBody] AssignmentDto dto)
        {
            var product = await _context.Products.FindAsync(dto.ProductId);
            var user = await _context.Users.FindAsync(dto.UserId);
            var state = await _context.States.FindAsync(dto.StateId);

            if (product == null || user == null || state == null)
                return BadRequest("Invalid data.");

            var existing = await _context.MaterialManagement
                .FirstOrDefaultAsync(m => m.ProductId == dto.ProductId);

            if (existing != null) _context.MaterialManagement.Remove(existing);

            var newAssignment = new MaterialManagement
            {
                ProductId = dto.ProductId,
                UserId = dto.UserId,
                StateId = dto.StateId,
                AssignmentDate = DateTime.Now
            };

            _context.MaterialManagement.Add(newAssignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Product assigned." });
        }

        // DELETE: api/MaterialManagements/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.MaterialManagement.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.MaterialManagement.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Assignment deleted." });
        }
    }

    public class SignatureDto
    {
        public int Id { get; set; }
        public string Signature { get; set; } = string.Empty;
    }

    public class AssignmentDto
    {
        public int ProductId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public int StateId { get; set; }
    }
}
