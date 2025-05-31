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
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 API AssignProduct - ProductId: {dto.ProductId}, UserId: {dto.UserId}, StateId: {dto.StateId}");

                // Vérifier le produit
                System.Diagnostics.Debug.WriteLine("🔍 Recherche produit...");
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Produit non trouvé: {dto.ProductId}");
                    return BadRequest("Product not found.");
                }
                System.Diagnostics.Debug.WriteLine($"✅ Produit trouvé: {product.Name}");

                // Vérifier l'utilisateur Identity
                System.Diagnostics.Debug.WriteLine("🔍 Recherche utilisateur Identity...");
                var identityUser = await _userManager.FindByEmailAsync(dto.UserId);
                if (identityUser == null)
                {
                    System.Diagnostics.Debug.WriteLine("🔍 Pas trouvé par email, essai par username...");
                    identityUser = await _userManager.FindByNameAsync(dto.UserId);
                }

                if (identityUser == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ Utilisateur Identity non trouvé: {dto.UserId}");
                    return BadRequest("User not found.");
                }
                System.Diagnostics.Debug.WriteLine($"✅ Utilisateur trouvé: {identityUser.Email}, ID: {identityUser.Id}");

                // Vérifier l'état
                System.Diagnostics.Debug.WriteLine("🔍 Recherche état...");
                var state = await _context.States.FindAsync(dto.StateId);
                if (state == null)
                {
                    System.Diagnostics.Debug.WriteLine($"❌ État non trouvé: {dto.StateId}");
                    return BadRequest("State not found.");
                }
                System.Diagnostics.Debug.WriteLine($"✅ État trouvé: {state.Status}");

                // Supprimer l'assignation existante
                System.Diagnostics.Debug.WriteLine("🔍 Recherche assignation existante...");
                var existing = await _context.MaterialManagement
                    .FirstOrDefaultAsync(m => m.ProductId == dto.ProductId);

                if (existing != null)
                {
                    System.Diagnostics.Debug.WriteLine($"🔄 Suppression assignation existante ID: {existing.Id}");
                    _context.MaterialManagement.Remove(existing);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("✅ Aucune assignation existante");
                }

                // Créer la nouvelle assignation
                System.Diagnostics.Debug.WriteLine("🔄 Création nouvelle assignation...");
                var newAssignment = new MaterialManagement
                {
                    ProductId = dto.ProductId,
                    UserId = identityUser.Id,
                    StateId = dto.StateId,
                    AssignmentDate = DateTime.Now,
                    Signature = "", // Initialiser avec valeur par défaut
                    SignatureDate = DateTime.MinValue // Initialiser avec valeur par défaut
                };

                System.Diagnostics.Debug.WriteLine($"🔄 Assignation créée - ProductId: {newAssignment.ProductId}, UserId: {newAssignment.UserId}, StateId: {newAssignment.StateId}");

                _context.MaterialManagement.Add(newAssignment);
                System.Diagnostics.Debug.WriteLine("✅ Assignation ajoutée au context");

                // Sauvegarder
                System.Diagnostics.Debug.WriteLine("🔄 Sauvegarde en cours...");
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine("✅ Sauvegarde réussie");

                System.Diagnostics.Debug.WriteLine($"✅ Assignation créée avec succès - ID: {newAssignment.Id}");
                return Ok(new { message = "Product assigned successfully.", assignmentId = newAssignment.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ ERREUR DÉTAILLÉE AssignProduct:");
                System.Diagnostics.Debug.WriteLine($"   Message: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($"   Type: {ex.GetType().Name}");
                System.Diagnostics.Debug.WriteLine($"   Stack: {ex.StackTrace}");

                if (ex.InnerException != null)
                {
                    System.Diagnostics.Debug.WriteLine($"   Inner Exception: {ex.InnerException.Message}");
                    System.Diagnostics.Debug.WriteLine($"   Inner Stack: {ex.InnerException.StackTrace}");
                }

                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message,
                    innerError = ex.InnerException?.Message
                });
            }
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

        // DELETE: api/MaterialManagements/product/5
        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> UnassignProduct(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"🔄 API UnassignProduct - ProductId: {productId}");

                var assignment = await _context.MaterialManagement
                    .FirstOrDefaultAsync(m => m.ProductId == productId);

                if (assignment == null)
                {
                    System.Diagnostics.Debug.WriteLine($"⚠️ Aucune assignation trouvée pour le produit {productId}");
                    return Ok(new { message = "No assignment found for this product." });
                }

                _context.MaterialManagement.Remove(assignment);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($"✅ Produit {productId} désassigné avec succès");
                return Ok(new { message = "Product unassigned successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"❌ Erreur UnassignProduct: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
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