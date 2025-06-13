using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Areas.Identity.Data;
using TechStockWeb.Data;
using TechStockWeb.Models;
using System.IdentityModel.Tokens.Jwt;

namespace TechStockWeb.Controllers
{

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

        [HttpGet("User")]
        public async Task<IActionResult> GetMyAssignments()
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(" GetMyAssignments API - START");

                string userId = null;

                if (Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    var token = authHeader.ToString().Replace("Bearer ", "");
                    System.Diagnostics.Debug.WriteLine($" Token received: {token.Substring(0, Math.Min(20, token.Length))}...");

                    try
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(token);

                        var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                        if (userIdClaim != null)
                        {
                            userId = userIdClaim.Value;
                            System.Diagnostics.Debug.WriteLine($" UserId extracted from token: '{userId}'");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(" Nameidentifier claim not found");

                            System.Diagnostics.Debug.WriteLine(" All token claims:");
                            foreach (var claim in jsonToken.Claims)
                            {
                                System.Diagnostics.Debug.WriteLine($"    {claim.Type}: {claim.Value}");
                            }
                        }
                    }
                    catch (Exception tokenEx)
                    {
                        System.Diagnostics.Debug.WriteLine($" Token decoding error: {tokenEx.Message}");
                        return Unauthorized("Invalid token");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(" No Authorization header");
                    return Unauthorized("No authorization header");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine(" UserId not found in token");
                    return Unauthorized("User ID not found in token");
                }

                System.Diagnostics.Debug.WriteLine($" Searching assignments for userId: '{userId}'");
                var assignments = await _context.MaterialManagement
                    .Include(m => m.Product)
                    .Include(m => m.State)
                    .Where(m => m.UserId == userId)
                    .ToListAsync();

                System.Diagnostics.Debug.WriteLine($" Number of assignments found: {assignments.Count}");

                if (assignments.Any())
                {
                    foreach (var assignment in assignments)
                    {
                        System.Diagnostics.Debug.WriteLine($"   - ID: {assignment.Id}, Product: {assignment.Product?.Name}, UserId: '{assignment.UserId}', Signed: {!string.IsNullOrEmpty(assignment.Signature)}");
                    }
                }
                else
                {
                    var allAssignments = await _context.MaterialManagement.Select(m => new { m.Id, m.UserId }).ToListAsync();
                    System.Diagnostics.Debug.WriteLine($" ALL assignments in database ({allAssignments.Count}):");
                    foreach (var assignment in allAssignments)
                    {
                        System.Diagnostics.Debug.WriteLine($"    ID: {assignment.Id}, UserId: '{assignment.UserId}'");
                    }
                }

                System.Diagnostics.Debug.WriteLine(" GetMyAssignments API - END");
                return Ok(assignments);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" GetMyAssignments error: {ex.Message}");
                return StatusCode(500, new { message = "Internal server error", error = ex.Message });
            }
        }

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

        [HttpPost("Sign")]
        public async Task<IActionResult> SignProduct([FromBody] SignatureDto dto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine(" SignProduct API - START");
                System.Diagnostics.Debug.WriteLine($" Assignment ID: {dto.Id}");
                System.Diagnostics.Debug.WriteLine($" Signature: {dto.Signature?.Substring(0, Math.Min(20, dto.Signature?.Length ?? 0))}...");

                string userId = null;

                if (Request.Headers.TryGetValue("Authorization", out var authHeader))
                {
                    var token = authHeader.ToString().Replace("Bearer ", "");
                    System.Diagnostics.Debug.WriteLine($" Token received: {token.Substring(0, Math.Min(20, token.Length))}...");

                    try
                    {
                        var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                        var jsonToken = handler.ReadJwtToken(token);

                        var userIdClaim = jsonToken.Claims.FirstOrDefault(x => x.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/nameidentifier");
                        if (userIdClaim != null)
                        {
                            userId = userIdClaim.Value;
                            System.Diagnostics.Debug.WriteLine($" UserId extracted from token: '{userId}'");
                        }
                        else
                        {
                            System.Diagnostics.Debug.WriteLine(" Nameidentifier claim not found");
                            return Unauthorized("Invalid token claims");
                        }
                    }
                    catch (Exception tokenEx)
                    {
                        System.Diagnostics.Debug.WriteLine($" Token decoding error: {tokenEx.Message}");
                        return Unauthorized("Invalid token");
                    }
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(" No Authorization header");
                    return Unauthorized("No authorization header");
                }

                if (string.IsNullOrEmpty(userId))
                {
                    System.Diagnostics.Debug.WriteLine(" UserId not found in token");
                    return Unauthorized("User ID not found in token");
                }

                System.Diagnostics.Debug.WriteLine($" Searching assignment {dto.Id} for user '{userId}'");
                var assignment = await _context.MaterialManagement
                    .Include(m => m.Product)
                    .FirstOrDefaultAsync(m => m.Id == dto.Id);

                if (assignment == null)
                {
                    System.Diagnostics.Debug.WriteLine($" Assignment {dto.Id} not found");
                    return NotFound("Assignment not found");
                }

                System.Diagnostics.Debug.WriteLine($" Assignment found: Product={assignment.Product?.Name}, UserId='{assignment.UserId}'");

                if (assignment.UserId != userId)
                {
                    System.Diagnostics.Debug.WriteLine($" Assignment belongs to '{assignment.UserId}', not to '{userId}'");
                    return Forbid("You can only sign your own assignments");
                }

                if (!string.IsNullOrEmpty(assignment.Signature))
                {
                    System.Diagnostics.Debug.WriteLine(" Assignment already signed");
                    return BadRequest("Assignment already signed");
                }

                System.Diagnostics.Debug.WriteLine(" Saving signature...");
                assignment.Signature = dto.Signature;
                assignment.SignatureDate = DateTime.Now;

                _context.Update(assignment);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($" Signature saved: {assignment.Signature}");
                System.Diagnostics.Debug.WriteLine($" Signature date: {assignment.SignatureDate}");

                return Ok(new
                {
                    message = "Signature saved successfully.",
                    assignmentId = assignment.Id,
                    signatureDate = assignment.SignatureDate
                });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" SignProduct error: {ex.Message}");
                System.Diagnostics.Debug.WriteLine($" Stack: {ex.StackTrace}");
                return StatusCode(500, new
                {
                    message = "Internal server error",
                    error = ex.Message
                });
            }
        }

        [HttpPost("Assign")]
        public async Task<IActionResult> AssignProduct([FromBody] AssignmentDto dto)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($" API AssignProduct - ProductId: {dto.ProductId}, UserId: {dto.UserId}, StateId: {dto.StateId}");

                System.Diagnostics.Debug.WriteLine(" Searching product...");
                var product = await _context.Products.FindAsync(dto.ProductId);
                if (product == null)
                {
                    System.Diagnostics.Debug.WriteLine($" Product not found: {dto.ProductId}");
                    return BadRequest("Product not found.");
                }
                System.Diagnostics.Debug.WriteLine($"✅ Product found: {product.Name}");

                System.Diagnostics.Debug.WriteLine(" Searching Identity user...");
                var identityUser = await _userManager.FindByEmailAsync(dto.UserId);
                if (identityUser == null)
                {
                    System.Diagnostics.Debug.WriteLine(" Not found by email, trying by username...");
                    identityUser = await _userManager.FindByNameAsync(dto.UserId);
                }

                if (identityUser == null)
                {
                    System.Diagnostics.Debug.WriteLine($" Identity user not found: {dto.UserId}");
                    return BadRequest("User not found.");
                }
                System.Diagnostics.Debug.WriteLine($" User found: {identityUser.Email}, ID: {identityUser.Id}");

                System.Diagnostics.Debug.WriteLine(" Searching state...");
                var state = await _context.States.FindAsync(dto.StateId);
                if (state == null)
                {
                    System.Diagnostics.Debug.WriteLine($" State not found: {dto.StateId}");
                    return BadRequest("State not found.");
                }
                System.Diagnostics.Debug.WriteLine($" State found: {state.Status}");

                System.Diagnostics.Debug.WriteLine(" Searching existing assignment...");
                var existing = await _context.MaterialManagement
                    .FirstOrDefaultAsync(m => m.ProductId == dto.ProductId);

                if (existing != null)
                {
                    System.Diagnostics.Debug.WriteLine($" Removing existing assignment ID: {existing.Id}");
                    _context.MaterialManagement.Remove(existing);
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine(" No existing assignment");
                }

                System.Diagnostics.Debug.WriteLine(" Creating new assignment...");
                var newAssignment = new MaterialManagement
                {
                    ProductId = dto.ProductId,
                    UserId = identityUser.Id,
                    StateId = dto.StateId,
                    AssignmentDate = DateTime.Now,
                    Signature = "",
                    SignatureDate = DateTime.MinValue
                };

                System.Diagnostics.Debug.WriteLine($" Assignment created - ProductId: {newAssignment.ProductId}, UserId: {newAssignment.UserId}, StateId: {newAssignment.StateId}");

                _context.MaterialManagement.Add(newAssignment);
                System.Diagnostics.Debug.WriteLine(" Assignment added to context");

                System.Diagnostics.Debug.WriteLine(" Saving...");
                await _context.SaveChangesAsync();
                System.Diagnostics.Debug.WriteLine(" Save successful");

                System.Diagnostics.Debug.WriteLine($" Assignment created successfully - ID: {newAssignment.Id}");
                return Ok(new { message = "Product assigned successfully.", assignmentId = newAssignment.Id });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" DETAILED ERROR AssignProduct:");
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

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var assignment = await _context.MaterialManagement.FindAsync(id);
            if (assignment == null) return NotFound();

            _context.MaterialManagement.Remove(assignment);
            await _context.SaveChangesAsync();

            return Ok(new { message = "Assignment deleted." });
        }

        [HttpDelete("product/{productId}")]
        public async Task<IActionResult> UnassignProduct(int productId)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($" API UnassignProduct - ProductId: {productId}");

                var assignment = await _context.MaterialManagement
                    .FirstOrDefaultAsync(m => m.ProductId == productId);

                if (assignment == null)
                {
                    System.Diagnostics.Debug.WriteLine($" No assignment found for product {productId}");
                    return Ok(new { message = "No assignment found for this product." });
                }

                _context.MaterialManagement.Remove(assignment);
                await _context.SaveChangesAsync();

                System.Diagnostics.Debug.WriteLine($" Product {productId} unassigned successfully");
                return Ok(new { message = "Product unassigned successfully." });
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($" UnassignProduct error: {ex.Message}");
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