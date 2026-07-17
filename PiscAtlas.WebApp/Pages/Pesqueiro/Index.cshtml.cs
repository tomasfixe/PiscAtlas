using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Pesqueiro
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PiscAtlas.Models.Models.Pesqueiro> Pesqueiros { get; set; } = new();

        // Propriedade para filtrar pesqueiros por tipo
        [BindProperty(SupportsGet = true)]
        public TipoPesqueiro? Tipo { get; set; }

        public IEnumerable<TipoPesqueiro> TiposDisponiveis { get; set; } = Enum.GetValues<TipoPesqueiro>();

        public async Task OnGetAsync()
        {
            // Carrega pesqueiros com as suas capturas associadas
            var query = _context.Pesqueiros.Include(p => p.Capturas).AsQueryable();

            // Aplica filtro de tipo se selecionado
            if (Tipo.HasValue)
            {
                query = query.Where(p => p.Tipo == Tipo.Value);
            }

            // Ordena alfabeticamente
            Pesqueiros = await query.OrderBy(p => p.Nome).ToListAsync();
        }
    }
}