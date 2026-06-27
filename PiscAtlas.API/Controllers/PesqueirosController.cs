using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PesqueirosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public PesqueirosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. READ ALL 
        [HttpGet]
        public async Task<IActionResult> GetPesqueiros()
        {
            var pesqueiros = await _context.Pesqueiros.ToListAsync();
            return Ok(pesqueiros);
        }

        // 2. READ ONE 
        [HttpGet("{id}")]
        public async Task<IActionResult> GetPesqueiro(int id)
        {
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);

            if (pesqueiro == null)
            {
                return NotFound(new { mensagem = "Pesqueiro não encontrado!" });
            }

            return Ok(pesqueiro);
        }

        // 3. CREATE 
        [HttpPost]
        public async Task<IActionResult> PostPesqueiro(Pesqueiro pesqueiro)
        {
            _context.Pesqueiros.Add(pesqueiro);
            await _context.SaveChangesAsync();

            
            return CreatedAtAction(nameof(GetPesqueiro), new { id = pesqueiro.PesqueiroId }, pesqueiro);
        }

        // 4. UPDATE 
        [HttpPut("{id}")]
        public async Task<IActionResult> PutPesqueiro(int id, Pesqueiro pesqueiro)
        {
            
            if (id != pesqueiro.PesqueiroId)
            {
                return BadRequest(new { mensagem = "O ID do URL não corresponde ao ID do objeto." });
            }

            _context.Entry(pesqueiro).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!PesqueiroExists(id))
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

        // 5. DELETE 
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeletePesqueiro(int id)
        {
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);
            if (pesqueiro == null)
            {
                return NotFound();
            }

            _context.Pesqueiros.Remove(pesqueiro);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        // Método auxiliar 
        private bool PesqueiroExists(int id)
        {
            
            return _context.Pesqueiros.Any(e => e.PesqueiroId == id);
        }
    }
}