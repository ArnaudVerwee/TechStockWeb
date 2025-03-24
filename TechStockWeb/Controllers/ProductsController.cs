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
        public async Task<IActionResult> Index(string SearchName, string SearchSerialNumber, string SearchType, string SearchSupplier)
        {
            var products = _context.Product
                .Include(p => p.Supplier)
                .Include(p => p.TypeArticle)
                .AsQueryable();

            // Appliquer les filtres si l'utilisateur a saisi des valeurs
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
                int typeId = int.Parse(SearchType);
                products = products.Where(p => p.TypeId == typeId);
            }

            if (!string.IsNullOrEmpty(SearchSupplier))
            {
                int supplierId = int.Parse(SearchSupplier);
                products = products.Where(p => p.SupplierId == supplierId);
            }

            // Stocker les valeurs de recherche dans ViewData pour les réafficher
            ViewData["SearchName"] = SearchName;
            ViewData["SearchSerialNumber"] = SearchSerialNumber;
            ViewData["SearchType"] = SearchType;
            ViewData["SearchSupplier"] = SearchSupplier;

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
    }
}
