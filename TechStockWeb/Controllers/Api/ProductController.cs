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
                var newProduct = new Product
                {
                    Name = product.Name,
                    SerialNumber = product.SerialNumber,
                    TypeId = product.TypeId,
                    SupplierId = product.SupplierId,
                    AssignedUserId = product.AssignedUserId
                };

                var typeExists = await _context.TypeArticle.AnyAsync(t => t.Id == product.TypeId);
                var supplierExists = await _context.Supplier.AnyAsync(s => s.Id == product.SupplierId);

                if (!typeExists)
                {
                    return BadRequest($"TypeArticle with ID {product.TypeId} does not exist.");
                }

                if (!supplierExists)
                {
                    return BadRequest($"Supplier with ID {product.SupplierId} does not exist.");
                }

                _context.Products.Add(newProduct);
                await _context.SaveChangesAsync();

                var createdProduct = await _context.Products
                    .Include(p => p.TypeArticle)
                    .Include(p => p.Supplier)
                    .Include(p => p.AssignedUser)
                    .FirstOrDefaultAsync(p => p.Id == newProduct.Id);

                return CreatedAtAction(nameof(GetProduct), new { id = newProduct.Id }, createdProduct);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error creating product: {ex.Message}");
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
                var existingProduct = await _context.Products.FindAsync(id);
                if (existingProduct == null)
                {
                    return NotFound();
                }

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
                return BadRequest($"Error updating product: {ex.Message}");
            }
        }

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
            try
            {
                Console.WriteLine($"API Filter - Received parameters:");
                Console.WriteLine($"  name: '{name}'");
                Console.WriteLine($"  serialNumber: '{serialNumber}'");
                Console.WriteLine($"  typeId: {typeId}");
                Console.WriteLine($"  supplierId: {supplierId}");
                Console.WriteLine($"  userId: '{userId}'");

                if (!string.IsNullOrEmpty(userId))
                {
                    var decodedUserId = Uri.UnescapeDataString(userId);
                    Console.WriteLine($"  userId decoded: '{decodedUserId}'");
                    userId = decodedUserId;
                }

                var query = _context.Products
                    .Include(p => p.TypeArticle)
                    .Include(p => p.Supplier)
                    .AsQueryable();

                if (!string.IsNullOrWhiteSpace(name))
                    query = query.Where(p => p.Name.Contains(name));

                if (!string.IsNullOrWhiteSpace(serialNumber))
                    query = query.Where(p => p.SerialNumber.Contains(serialNumber));

                if (typeId.HasValue)
                    query = query.Where(p => p.TypeId == typeId.Value);

                if (supplierId.HasValue)
                    query = query.Where(p => p.SupplierId == supplierId.Value);

                if (!string.IsNullOrWhiteSpace(userId))
                {
                    Console.WriteLine($"Filtering by userId: '{userId}'");

                    if (userId == "NotAssigned")
                    {
                        var assignedProductIds = _context.MaterialManagement.Select(m => m.ProductId).ToList();
                        query = query.Where(p => !assignedProductIds.Contains(p.Id));
                        Console.WriteLine("Filtering for NOT ASSIGNED products");
                    }
                    else if (userId != "All")
                    {
                        var assignedProductIds = _context.MaterialManagement
                            .Where(m => m.UserId == userId)
                            .Select(m => m.ProductId)
                            .ToList();

                        query = query.Where(p => assignedProductIds.Contains(p.Id));

                        Console.WriteLine($"Found {assignedProductIds.Count} products assigned to user '{userId}'");
                        Console.WriteLine($"Assigned product IDs: [{string.Join(", ", assignedProductIds)}]");
                    }
                }

                var products = await query.ToListAsync();
                Console.WriteLine($"Query returned {products.Count} products");

                var materialAssignments = await _context.MaterialManagement
                    .Include(m => m.User)
                    .ToListAsync();

                Console.WriteLine($"Total material assignments: {materialAssignments.Count}");

                var result = products.Select(p => {
                    var assignment = materialAssignments.FirstOrDefault(m => m.ProductId == p.Id);

                    string assignedUserName = null;
                    if (assignment?.User != null && !string.IsNullOrEmpty(assignment.User.UserName))
                    {
                        assignedUserName = assignment.User.UserName;
                    }

                    var productResult = new
                    {
                        p.Id,
                        p.Name,
                        p.SerialNumber,
                        p.TypeId,
                        p.SupplierId,
                        p.AssignedUserId,
                        TypeName = p.TypeArticle?.Name,
                        SupplierName = p.Supplier?.Name,
                        AssignedUserName = assignedUserName
                    };

                    Console.WriteLine($"Product '{p.Name}' -> AssignedUserName: '{productResult.AssignedUserName}' (IsNull: {productResult.AssignedUserName == null})");
                    return productResult;
                }).ToList();

                Console.WriteLine($"Final result: {result.Count} products returned");
                return Ok(result);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"API Error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                return StatusCode(500, $"Internal error: {ex.Message}");
            }
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
                    return NotFound(new { message = "This product is not assigned" });
                }

                _context.MaterialManagement.Remove(assignment);
                await _context.SaveChangesAsync();

                return Ok(new { message = "Product unassigned successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = $"Error: {ex.Message}" });
            }
        }
    }
}