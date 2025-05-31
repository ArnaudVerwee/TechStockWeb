using Microsoft.AspNetCore.Mvc;
using TechStockWeb.Models;
using TechStockWeb.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authorization;

namespace TechStockWeb.Controllers.Api
{
    [Route("api/[controller]")]
    [ApiController]
    
    public class ProductController : ControllerBase
    {
        private readonly TechStockContext _context;

        public ProductController(TechStockContext context)
        {
            _context = context;
        }

        // GET: api/Product
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Product>>> GetProducts()
        {
            var products = await _context.Products
                .Include(p => p.TypeArticle)
                .Include(p => p.Supplier)
                .Include(p => p.AssignedUser)
                .ToListAsync();
            return Ok(products);
        }

        // GET: api/Product/5
        [HttpGet("{id}")]
        public async Task<ActionResult<Product>> GetProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.TypeArticle)
                .Include(p => p.Supplier)
                .Include(p => p.AssignedUser)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            return Ok(product);
        }

        [HttpPost]
        public async Task<ActionResult<Product>> PostProduct(Product product)
        {
            try
            {
                // SOLUTION: Créer un nouveau produit avec seulement les IDs
                var newProduct = new Product
                {
                    Name = product.Name,
                    SerialNumber = product.SerialNumber,
                    TypeId = product.TypeId,
                    SupplierId = product.SupplierId,
                    AssignedUserId = product.AssignedUserId
                    // Ne pas inclure TypeArticle et Supplier - seulement les IDs
                };

                // Vérifier que les IDs existent
                var typeExists = await _context.TypeArticle.AnyAsync(t => t.Id == product.TypeId);
                var supplierExists = await _context.Supplier.AnyAsync(s => s.Id == product.SupplierId);

                if (!typeExists)
                {
                    return BadRequest($"TypeArticle avec l'ID {product.TypeId} n'existe pas.");
                }

                if (!supplierExists)
                {
                    return BadRequest($"Supplier avec l'ID {product.SupplierId} n'existe pas.");
                }

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                // Récupérer le produit créé avec toutes les relations
                var createdProduct = await _context.Products
                    .Include(p => p.TypeArticle)
                    .Include(p => p.Supplier)
                    .Include(p => p.AssignedUser)
                    .FirstOrDefaultAsync(p => p.Id == newProduct.Id);

                return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de la création du produit: {ex.Message}");
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutProduct(int id, Product product)
        {
            if (id != product.Id)
            {
                return BadRequest();
            }

            try
            {
                // Récupérer le produit existant
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

                // Mettre à jour seulement les propriétés nécessaires
                existingProduct.Name = product.Name;
                existingProduct.SerialNumber = product.SerialNumber;
                existingProduct.TypeId = product.TypeId;
                existingProduct.SupplierId = product.SupplierId;
                existingProduct.AssignedUserId = product.AssignedUserId;

                await _context.SaveChangesAsync();
                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest($"Erreur lors de la mise à jour: {ex.Message}");
            }
        }

        // DELETE: api/Product/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        [HttpGet("filter")]
        public async Task<ActionResult<IEnumerable<object>>> GetProductsFilter(
    [FromQuery] string? name = null,
    [FromQuery] string? serialNumber = null,
    [FromQuery] int? typeId = null,
    [FromQuery] int? supplierId = null,
    [FromQuery] string? userId = null)
        {
            var query = _context.Products
                .Include(p => p.TypeArticle)
                .Include(p => p.Supplier)
                .AsQueryable();

            // ✅ FILTRES NORMAUX
            if (!string.IsNullOrWhiteSpace(name))
                query = query.Where(p => p.Name.Contains(name));

            if (!string.IsNullOrWhiteSpace(serialNumber))
                query = query.Where(p => p.SerialNumber.Contains(serialNumber));

            if (typeId.HasValue)
                query = query.Where(p => p.TypeId == typeId.Value);

            if (supplierId.HasValue)
                query = query.Where(p => p.SupplierId == supplierId.Value);

            // ✅ FILTRE UTILISATEUR AVEC MATERIALMANAGEMENT
            if (!string.IsNullOrWhiteSpace(userId))
            {
                if (userId == "NotAssigned")
                {
                    // Produits NON assignés
                    query = from p in query
                            join m in _context.MaterialManagement on p.Id equals m.ProductId into assignments
                            from assignment in assignments.DefaultIfEmpty()
                            where assignment == null
                            select p;
                }
                else if (userId != "All")
                {
                    // Produits assignés à un utilisateur spécifique
                    query = from p in query
                            join m in _context.MaterialManagement on p.Id equals m.ProductId into assignments
                            from assignment in assignments.DefaultIfEmpty()
                            where assignment != null && assignment.UserId == userId
                            select p;
                }
            }

            var products = await query.ToListAsync();

            // ✅ RÉCUPÉRER LES ASSIGNATIONS SÉPARÉMENT
            var materialAssignments = await _context.MaterialManagement
                .Include(m => m.User)
                .ToListAsync();

            var result = products.Select(p => {
                // ✅ TROUVER L'ASSIGNATION POUR CE PRODUIT
                var assignment = materialAssignments.FirstOrDefault(m => m.ProductId == p.Id);

                return new
                {
                    p.Id,
                    p.Name,
                    p.SerialNumber,
                    p.TypeId,
                    p.SupplierId,
                    p.AssignedUserId, // Garde pour compatibilité (sera null)
                    TypeName = p.TypeArticle?.Name,
                    SupplierName = p.Supplier?.Name,
                    // ✅ ASSIGNATION VIA MATERIALMANAGEMENT
                    AssignedUserName = assignment?.User?.UserName
                };
            });

            return Ok(result);
        }
        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> UnassignByProductId(int productId)
        {
            try
            {
                var assignment = await _context.MaterialManagement
                    .FirstOrDefaultAsync(m => m.ProductId == productId);

                if (assignment == null)
                {
                    return NotFound(new { message = "Ce produit n'est pas assigné" });
                }

                _context.MaterialManagement.Remove(assignment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Produit désassigné avec succès" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Erreur: {ex.Message}" });
            }
        }
    }
}