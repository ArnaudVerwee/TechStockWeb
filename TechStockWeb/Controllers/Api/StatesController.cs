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
    public class StatesController : ControllerBase
    {
        private readonly TechStockContext _context;

        public StatesController(TechStockContext context)
        {
            _context = context;
        }

        // GET: api/states
        [HttpGet]
        public async Task<ActionResult<IEnumerable<States>>> GetStates()
        {
            return await _context.States.ToListAsync();
        }

        // GET: api/states/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<States>> GetState(int id)
        {
            var state = await _context.States.FindAsync(id);

            if (state == null)
            {
                return NotFound();
            }

            return state;
        }

        // POST: api/states
        [HttpPost]
        public async Task<ActionResult<States>> CreateState([FromBody] States state)
        {
            if (ModelState.IsValid)
            {
                _context.States.Add(state);
                await _context.SaveChangesAsync();

                return CreatedAtAction(nameof(GetState), new { id = state.Id }, state);
            }

            return BadRequest(ModelState);
        }

        // PUT: api/states/{id}
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateState(int id, [FromBody] States state)
        {
            if (id != state.Id)
            {
                return BadRequest();
            }

            _context.Entry(state).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!StateExists(id))
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

        // DELETE: api/states/{id}
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteState(int id)
        {
            var state = await _context.States.FindAsync(id);
            if (state == null)
            {
                return NotFound();
            }

            _context.States.Remove(state);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool StateExists(int id)
        {
            return _context.States.Any(e => e.Id == id);
        }
    }
}
