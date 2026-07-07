using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public IEnumerable<PiscAtlas.Models.Models.Pesqueiro> Pesqueiros { get; set; } = new List<PiscAtlas.Models.Models.Pesqueiro>();
        public IEnumerable<PiscAtlas.Models.Models.Captura> Capturas { get; set; } = new List<PiscAtlas.Models.Models.Captura>();

        public async Task OnGetAsync()
        {
            // 1. Buscar todos os pesqueiros para os Pins do Mapa
            Pesqueiros = await _context.Pesqueiros.ToListAsync();

            // 2. Buscar as últimas 50 capturas
            Capturas = await _context.Capturas
                .Include(c => c.Utilizador)
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Where(c => !c.FraudeConfirmada)
                .OrderByDescending(c => c.DataCaptura)
                .Take(50)
                .ToListAsync();
        }
    }
}