using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CapturasController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public CapturasController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<IActionResult> GetCapturas()
        {
            // Nas capturas, é útil trazer também os dados da espécie e do pesqueiro associado
            var capturas = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .ToListAsync();

            return Ok(capturas);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetCaptura(int id)
        {
            var captura = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .FirstOrDefaultAsync(c => c.CapturaId == id);

            if (captura == null) return NotFound();

            return Ok(captura);
        }

        [HttpPost]
        public async Task<IActionResult> PostCaptura(Captura captura)
        {
            _context.Capturas.Add(captura);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetCaptura), new { id = captura.CapturaId }, captura);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> PutCaptura(int id, Captura captura)
        {
            if (id != captura.CapturaId) return BadRequest(new { mensagem = "IDs não coincidem." });

            _context.Entry(captura).State = EntityState.Modified;

            try { await _context.SaveChangesAsync(); }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Capturas.Any(e => e.CapturaId == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCaptura(int id)
        {
            var captura = await _context.Capturas.FindAsync(id);
            if (captura == null) return NotFound();

            _context.Capturas.Remove(captura);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}