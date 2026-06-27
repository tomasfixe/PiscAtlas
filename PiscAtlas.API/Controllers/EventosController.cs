using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public EventosController(ApplicationDbContext context)
        {
            _context = context;
        }

        // 1. LER TODOS OS EVENTOS
        // GET: api/Eventos
        [HttpGet]
        public async Task<IActionResult> GetEventos()
        {
            // Trazemos os eventos e incluímos a espécie alvo para a App Android saber qual é o peixe
            var eventos = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .OrderBy(e => e.DataInicio)
                .ToListAsync();

            return Ok(eventos);
        }

        // 2. LER UM EVENTO ESPECÍFICO
        // GET: api/Eventos/5
        [HttpGet("{id}")]
        public async Task<IActionResult> GetEvento(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes) // Útil para a App saber quem já está inscrito
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            return Ok(evento);
        }

        // 3. CRIAR UM NOVO EVENTO
        // POST: api/Eventos
        [HttpPost]
        public async Task<IActionResult> PostEvento(Evento evento)
        {
            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            return CreatedAtAction(nameof(GetEvento), new { id = evento.EventoId }, evento);
        }

        // 4. ATUALIZAR UM EVENTO EXISTENTE
        // PUT: api/Eventos/5
        [HttpPut("{id}")]
        public async Task<IActionResult> PutEvento(int id, Evento evento)
        {
            if (id != evento.EventoId) return BadRequest(new { mensagem = "IDs não coincidem." });

            _context.Entry(evento).State = EntityState.Modified;

            try
            {
                await _context.SaveChangesAsync();
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_context.Eventos.Any(e => e.EventoId == id)) return NotFound();
                else throw;
            }
            return NoContent();
        }

        // 5. APAGAR UM EVENTO (Com proteção de inscrições baseada no seu MVC)
        // DELETE: api/Eventos/5
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteEvento(int id)
        {
            // Tal como no seu MVC, precisamos de trazer as Inscrições para as apagar primeiro
            var evento = await _context.Eventos
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            // Apaga as inscrições associadas para não quebrar a base de dados
            if (evento.Inscricoes != null && evento.Inscricoes.Any())
            {
                _context.Inscricoes.RemoveRange(evento.Inscricoes);
            }

            // Agora sim, apaga o evento
            _context.Eventos.Remove(evento);
            await _context.SaveChangesAsync();

            return NoContent();
        }
    }
}