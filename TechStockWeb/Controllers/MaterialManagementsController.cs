using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;
using TechStockWeb.Models;

namespace TechStockWeb.Controllers
{
    public class MaterialManagementsController : Controller
    {
        private readonly TechStockContext _context;

        public MaterialManagementsController(TechStockContext context)
        {
            _context = context;
        }

        // GET: MaterialManagements
        public async Task<IActionResult> Index()
        {
            var techStockContext = _context.MaterialManagement
                .Include(m => m.Product)
                .Include(m => m.State)
                .Include(m => m.User);
            return View(await techStockContext.ToListAsync());
        }

        // ✅ GET: MaterialManagements/AssignToUser/{id}
        public async Task<IActionResult> AssignToUser(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            ViewBag.Users = await _context.Users.ToListAsync();
            ViewBag.States = await _context.States.ToListAsync();
            return View(product);
        }

        // ✅ POST: MaterialManagements/AssignToUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToUser(int id, string userId, int stateId)
        {
            var product = await _context.Product.FindAsync(id);
            var user = await _context.Users.FindAsync(userId);
            var state = await _context.States.FindAsync(stateId);

            if (product == null || user == null || state == null)
            {
                return NotFound();
            }

            // Vérifier si ce produit est déjà assigné et le supprimer
            var existingAssignment = await _context.MaterialManagement
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (existingAssignment != null)
            {
                _context.MaterialManagement.Remove(existingAssignment);
            }

            // Créer une nouvelle assignation
            var assignment = new MaterialManagement
            {
                ProductId = id,
                UserId = userId,
                StateId = stateId,
                AssignmentDate = DateTime.Now,
                Signature = "Pending"
            };

            _context.MaterialManagement.Add(assignment);
            await _context.SaveChangesAsync();

            // Rediriger vers la page d'index des produits
            return RedirectToAction("Index", "Products");
        }


        // GET: MaterialManagements/Details/5
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

        // GET: MaterialManagements/Create
        public IActionResult Create()
        {
            ViewData["ProductId"] = new SelectList(_context.Product, "Id", "Id");
            ViewData["StateId"] = new SelectList(_context.States, "Id", "Id");
            ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id");
            return View();
        }

        // POST: MaterialManagements/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,UserId,ProductId,StateId,Signature,AssignmentDate,SignatureDate")] MaterialManagement materialManagement)
        {
            if (ModelState.IsValid)
            {
                _context.Add(materialManagement);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(materialManagement);
        }

        // GET: MaterialManagements/Edit/5
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

        // POST: MaterialManagements/Edit/5
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

        // GET: MaterialManagements/Delete/5
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

        // POST: MaterialManagements/Delete/5
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
