using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;

namespace PiscAtlas.WebApp.Pages.Evento
{
    
    [Authorize(Roles = "Admin")]
    public class EliminarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EliminarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int EventoId { get; set; }

        public PiscAtlas.Models.Models.Evento EventoItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Carrega o evento com as dependências necessárias para exibição
            var evento = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            EventoItem = evento;
            EventoId = evento.EventoId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var evento = await _context.Eventos
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == EventoId);

            if (evento != null)
            {
                // Remove as inscrições associadas para evitar erros de integridade na base de dados
                if (evento.Inscricoes != null && evento.Inscricoes.Any())
                {
                    _context.Inscricoes.RemoveRange(evento.Inscricoes);
                }

                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Evento eliminado com sucesso.";
            }

            return RedirectToPage("./Index");
        }
    }
}