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
    public class TypeArticlesController : Controller
    {
        private readonly TechStockContext _context;

        public TypeArticlesController(TechStockContext context)
        {
            _context = context;
        }

        // GET: TypeArticles
        public async Task<IActionResult> Index()
        {
            return View(await _context.TypeArticle.ToListAsync());
        }

        // GET: TypeArticles/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeArticle = await _context.TypeArticle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeArticle == null)
            {
                return NotFound();
            }

            return View(typeArticle);
        }

        // GET: TypeArticles/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: TypeArticles/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name")] TypeArticle typeArticle)
        {
            if (ModelState.IsValid)
            {
                _context.Add(typeArticle);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(typeArticle);
        }

        // GET: TypeArticles/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeArticle = await _context.TypeArticle.FindAsync(id);
            if (typeArticle == null)
            {
                return NotFound();
            }
            return View(typeArticle);
        }

        // POST: TypeArticles/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name")] TypeArticle typeArticle)
        {
            if (id != typeArticle.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(typeArticle);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TypeArticleExists(typeArticle.Id))
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
            return View(typeArticle);
        }

        // GET: TypeArticles/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var typeArticle = await _context.TypeArticle
                .FirstOrDefaultAsync(m => m.Id == id);
            if (typeArticle == null)
            {
                return NotFound();
            }

            return View(typeArticle);
        }

        // POST: TypeArticles/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var typeArticle = await _context.TypeArticle.FindAsync(id);
            if (typeArticle != null)
            {
                _context.TypeArticle.Remove(typeArticle);
            }

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TypeArticleExists(int id)
        {
            return _context.TypeArticle.Any(e => e.Id == id);
        }
    }
}
