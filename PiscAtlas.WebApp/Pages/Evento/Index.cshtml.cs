using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Evento
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PiscAtlas.Models.Models.Evento> Eventos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Eventos = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes)
                .OrderBy(e => e.DataInicio)
                .ToListAsync();
        }
    }
}