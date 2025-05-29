using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;
using TechStockWeb.Models;
using TechStockWeb.Areas.Identity.Data;

namespace TechStockWeb.Controllers
{
    [Authorize]
    public class MaterialManagementsController : Controller
    {
        private readonly TechStockContext _context;
        private readonly UserManager<TechStockWebUser> _userManager;

        public MaterialManagementsController(TechStockContext context, UserManager<TechStockWebUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        
        public async Task<IActionResult> Index()
        {
            var techStockContext = _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Include(m => m.User);
            return View(await techStockContext.ToListAsync());
        }

        public async Task<IActionResult> MyAssignedProducts()
        {
            var userId = _userManager.GetUserId(User);
            var assignedProducts = await _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Where(m => m.UserId == userId)
                .ToListAsync();

            return View(assignedProducts);
        }

       
        public async Task<IActionResult> SignProduct(int id)
        {
            var assignment = await _context.MaterialManagement
                .Include(m => m.Product)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (assignment == null || assignment.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            return View(assignment);
        }

       
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SignProduct(int id, string signature)
        {
            var assignment = await _context.MaterialManagement.FindAsync(id);
            if (assignment == null || assignment.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            assignment.Signature = signature;
            assignment.SignatureDate = DateTime.Now;

            _context.Update(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAssignedProducts));
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SaveSignature(int id, string signature)
        {
            if (string.IsNullOrEmpty(signature))
            {
                return BadRequest("Signature is required.");
            }

            var assignment = await _context.MaterialManagement.FindAsync(id);
            if (assignment == null || assignment.UserId != _userManager.GetUserId(User))
            {
                return NotFound();
            }

            assignment.Signature = signature;
            assignment.SignatureDate = DateTime.Now;

            _context.Update(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction(nameof(MyAssignedProducts));
        }

        
        public async Task<IActionResult> AssignToUser(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Users = await _userManager.Users.ToListAsync();
            ViewBag.States = await _context.States.ToListAsync();
            return View(product);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToUser(int id, string userId, int stateId)
        {
            var product = await _context.Products.FindAsync(id);
            var user = await _userManager.FindByIdAsync(userId);
            var state = await _context.States.FindAsync(stateId);

            if (product == null || user == null || state == null)
            {
                return NotFound();
            }

            var existingAssignment = await _context.MaterialManagement
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (existingAssignment != null)
            {
                _context.MaterialManagement.Remove(existingAssignment);
            }

            var assignment = new MaterialManagement
            {
                ProductId = id,
                UserId = userId,
                StateId = stateId,
                AssignmentDate = DateTime.Now,
                Signature = ""
            };

            _context.MaterialManagement.Add(assignment);
            await _context.SaveChangesAsync();

            return RedirectToAction("Index", "Products");
        }

       
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialManagement = await _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (materialManagement == null)
            {
                return NotFound();
            }

            return View(materialManagement);
        }

        
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialManagement = await _context.MaterialManagement.FindAsync(id);
            if (materialManagement == null)
            {
                return NotFound();
            }
            return View(materialManagement);
        }

        
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,UserId,ProductId,StateId,Signature,AssignmentDate,SignatureDate")] MaterialManagement materialManagement)
        {
            if (id != materialManagement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(materialManagement);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!MaterialManagementExists(materialManagement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(materialManagement);
        }

        
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var materialManagement = await _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Include(m => m.User)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (materialManagement == null)
            {
                return NotFound();
            }

            return View(materialManagement);
        }

        
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var materialManagement = await _context.MaterialManagement.FindAsync(id);
            if (materialManagement != null)
            {
                _context.MaterialManagement.Remove(materialManagement);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        
        private bool MaterialManagementExists(int id)
        {
            return _context.MaterialManagement.Any(e => e.Id == id);
        }
    }
}
