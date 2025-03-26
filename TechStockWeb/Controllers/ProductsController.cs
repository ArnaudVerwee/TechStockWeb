using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;
using TechStockWeb.Models;

namespace TechStockWeb.Controllers
{
    public class ProductsController : Controller
    {
        private readonly TechStockContext _context;

        public ProductsController(TechStockContext context)
        {
            _context = context;
        }

        // GET: Products
        public async Task<IActionResult> Index(string SearchName, string SearchSerialNumber, string SearchType, string SearchSupplier, string SearchUser)
        {
            // Charger les produits de base
            var products = _context.Product
                .Include(p => p.Supplier)
                .Include(p => p.TypeArticle)
                .AsQueryable();

            // Appliquer les filtres
            if (!string.IsNullOrEmpty(SearchName))
            {
                products = products.Where(p => p.Name.Contains(SearchName));
            }

            if (!string.IsNullOrEmpty(SearchSerialNumber))
            {
                products = products.Where(p => p.SerialNumber.Contains(SearchSerialNumber));
            }

            if (!string.IsNullOrEmpty(SearchType))
            {
                int typeId;
                if (int.TryParse(SearchType, out typeId)) // Vérifier si SearchType peut être converti en entier
                {
                    products = products.Where(p => p.TypeId == typeId);
                }
                else
                {
                    // Recherche par nom du type d'article si la conversion échoue
                    products = products.Where(p => p.TypeArticle.Name.Contains(SearchType));
                }
            }

            if (!string.IsNullOrEmpty(SearchSupplier))
            {
                int supplierId;
                if (int.TryParse(SearchSupplier, out supplierId)) // Vérifier si SearchSupplier peut être converti en entier
                {
                    products = products.Where(p => p.SupplierId == supplierId);
                }
                else
                {
                    // Recherche par nom du fournisseur si la conversion échoue
                    products = products.Where(p => p.Supplier.Name.Contains(SearchSupplier));
                }
            }

            if (!string.IsNullOrEmpty(SearchUser))
            {
                if (SearchUser == "NotAssigned")
                {
                    // Sélectionner les produits qui n'ont PAS d'assignation
                    products = from p in products
                               join m in _context.MaterialManagement on p.Id equals m.ProductId into assignments
                               from assignment in assignments.DefaultIfEmpty()
                               where assignment == null
                               select p;
                }
                else
                {
                    var userId = SearchUser;
                    products = from p in products
                               join m in _context.MaterialManagement on p.Id equals m.ProductId into assignments
                               from assignment in assignments.DefaultIfEmpty()
                               where assignment != null && assignment.UserId == userId
                               select p;
                }
            }


            // Charger les informations sur l'assignation des utilisateurs
            var materialAssignments = await _context.MaterialManagement
                .Include(m => m.User)
                .ToListAsync();

            // Charger la liste des utilisateurs pour le menu déroulant
            ViewBag.Users = new SelectList(await _context.Users.ToListAsync(), "Id", "UserName");

            ViewBag.MaterialAssignments = materialAssignments;

            // Stocker les valeurs de recherche dans ViewData pour les réafficher
            ViewData["SearchName"] = SearchName;
            ViewData["SearchSerialNumber"] = SearchSerialNumber;
            ViewData["SearchType"] = SearchType;
            ViewData["SearchSupplier"] = SearchSupplier;
            ViewData["SearchUser"] = SearchUser; // Stockage du filtre utilisateur

            // Charger les listes déroulantes pour les filtres
            ViewBag.TypeList = new SelectList(_context.TypeArticle, "Id", "Name");
            ViewBag.SupplierList = new SelectList(_context.Supplier, "Id", "Name");

            return View(await products.ToListAsync());
        }





        // GET: Products/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Supplier)
                .Include(p => p.TypeArticle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // GET: Products/Create
        public IActionResult Create()
        {
            ViewData["SupplierId"] = new SelectList(_context.Supplier, "Id", "Name");
            ViewData["TypeId"] = new SelectList(_context.TypeArticle, "Id", "Name");
            return View();
        }

        // POST: Products/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,SerialNumber,TypeId,SupplierId")] Product product)
        {
            Debug.WriteLine("Méthode Create appelée");
            Debug.WriteLine("Nom: "+ product.Name);
            Debug.WriteLine("SerialNumber: " + product.SerialNumber);
            Debug.WriteLine("TypeID: " + product.TypeId);
            Debug.WriteLine("SupplierID: " + product.SupplierId);
            Debug.WriteLine("ID : " + product.Id);
            var typeArticle = await _context.TypeArticle.FindAsync(product.TypeId);
            var supplier = await _context.Supplier.FindAsync(product.SupplierId);

            product.TypeArticle = typeArticle;
            product.Supplier = supplier;

            ModelState.Remove("TypeArticle");
            ModelState.Remove("Supplier"); 
            
            if (ModelState.IsValid)
            {
                _context.Add(product);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
 
            ViewData["SupplierId"] = new SelectList(_context.Supplier, "Id", "Name", product.SupplierId);
            ViewData["TypeId"] = new SelectList(_context.TypeArticle, "Id", "Name", product.TypeId);
            return View(product);
        }

        // GET: Products/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            ViewData["SupplierId"] = new SelectList(_context.Supplier, "Id", "Name", product.SupplierId);
            ViewData["TypeId"] = new SelectList(_context.TypeArticle, "Id", "Name", product.TypeId);
            return View(product);
        }

        // POST: Products/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,SerialNumber,TypeId,SupplierId")] Product product)
        {
            if (id != product.Id)
            {
                return NotFound();
            }

            // Récupération des objets TypeArticle et Supplier
            var typeArticle = await _context.TypeArticle.FindAsync(product.TypeId);
            var supplier = await _context.Supplier.FindAsync(product.SupplierId);

            // Assigner les objets trouvés
            product.TypeArticle = typeArticle;
            product.Supplier = supplier;

            // Supprimer les erreurs de validation liées aux propriétés non bindées
            ModelState.Remove("TypeArticle");
            ModelState.Remove("Supplier");

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProductExists(product.Id))
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

            ViewData["SupplierId"] = new SelectList(_context.Supplier, "Id", "Name", product.SupplierId);
            ViewData["TypeId"] = new SelectList(_context.TypeArticle, "Id", "Name", product.TypeId);
            return View(product);
        }


        // GET: Products/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var product = await _context.Product
                .Include(p => p.Supplier)
                .Include(p => p.TypeArticle)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (product == null)
            {
                return NotFound();
            }

            return View(product);
        }

        // POST: Products/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var product = await _context.Product.FindAsync(id);
            if (product != null)
            {
                _context.Product.Remove(product);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProductExists(int id)
        {
            return _context.Product.Any(e => e.Id == id);
        }

        // Assigner un produit à un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignToUser(int id, int userId)
        {
            var product = await _context.Product.FindAsync(id);
            var user = await _context.Users.FindAsync(userId);

            Debug.WriteLine(userId);
            if (product == null || user == null)
            {
                return NotFound();
            }

            // Vérifier si le produit est déjà assigné
            var existingAssignment = await _context.MaterialManagement
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (existingAssignment != null)
            {
                TempData["Error"] = "This product is already assigned to a user.";
                return RedirectToAction(nameof(Index));
            }

            // Création de l'assignation
            var assignment = new MaterialManagement
            {
                ProductId = id,
                //UserId = userId
            };

            _context.MaterialManagement.Add(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product assigned successfully.";
            return RedirectToAction(nameof(Index));
        }

        // Désassigner un produit d'un utilisateur
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnassignFromUser(int id)
        {
            var assignment = await _context.MaterialManagement
                .FirstOrDefaultAsync(m => m.ProductId == id);

            if (assignment == null)
            {
                TempData["Error"] = "This product is not assigned to any user.";
                return RedirectToAction(nameof(Index));
            }

            _context.MaterialManagement.Remove(assignment);
            await _context.SaveChangesAsync();

            TempData["Success"] = "Product unassigned successfully.";
            return RedirectToAction(nameof(Index));
        }

    }
}
