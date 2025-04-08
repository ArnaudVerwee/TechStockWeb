using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TechStockWeb.Data;
using TechStockWeb.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace TechStockWeb.ApiControllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TypeArticlesController : ControllerBase
    {
        private readonly TechStockContext _context;

        public TypeArticlesController(TechStockContext context)
        {
            _context = context;
        }

        // GET: api/typearticles
        [HttpGet]
        public async Task<ActionResult<IEnumerable<TypeArticle>>> GetTypeArticles()
        {
            return await _context.TypeArticle.ToListAsync();
        }

        // GET: api/typearticles/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<TypeArticle>> GetTypeArticle(int id)
        {
            var typeArticle = await _context.TypeArticle.FindAsync(id);

            if (typeArticle == null)
            {
                return NotFound();
            }

            return typeArticle;
        }

        // POST: api/typearticles
        [HttpPost]
        public async Task<ActionResult<TypeArticle>> CreateTypeArticle([FromBody] TypeArticle typeArticle)
        {
            if (ModelState.IsValid)
            {
                _context.TypeArticle.Add(typeArticle);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetTypeArticle), new { id = typeArticle.Id }, typeArticle);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/typearticles/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateTypeArticle(int id, [FromBody] TypeArticle typeArticle)
        {
            if (id != typeArticle.Id)
            {
                return BadRequest();
            }

            _context.Entry(typeArticle).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!TypeArticleExists(id))
                {
                    return NotFound();
                }
                else
                {
                    throw;
                }
            }

            return NoContent();
        }

        // DELETE: api/typearticles/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteTypeArticle(int id)
        {
            var typeArticle = await _context.TypeArticle.FindAsync(id);
            if (typeArticle == null)
            {
                return NotFound();
            }

            _context.TypeArticle.Remove(typeArticle);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool TypeArticleExists(int id)
        {
            return _context.TypeArticle.Any(e => e.Id == id);
        }
    }
}
