using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EspeciesController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EspeciesController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetEspecies()
        {
            var especies = await _context.Especies.ToListAsync();
            return Ok(especies);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetEspecie(int id)
        {
            var especie = await _context.Especies.FindAsync(id);
            if (especie == null) return NotFound();

            return Ok(especie);
        }

        [HttpPost]
        public async Task<IActionResult> PostEspecie(Especie especie)
        {
            _context.Especies.Add(especie);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEspecie), new { id = especie.EspecieId }, especie);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutEspecie(int id, Especie especie)
        {
            if (id != especie.EspecieId) return BadRequest(new { mensagem = "IDs não coincidem." });

            _context.Entry(especie).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Especies.Any(e => e.EspecieId == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEspecie(int id)
        {
            var especie = await _context.Especies.FindAsync(id);
            if (especie == null) return NotFound();

            _context.Especies.Remove(especie);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}