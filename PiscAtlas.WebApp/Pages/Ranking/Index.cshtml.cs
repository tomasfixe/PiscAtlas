using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Ranking
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public IndexModel(ApplicationDbContext context)
        {
            _context = context;
        }

        // Filtros recebidos pelo URL
        [BindProperty(SupportsGet = true)]
        public string TempoFiltro { get; set; } = "Sempre";

        [BindProperty(SupportsGet = true)]
        public int? PesqueiroId { get; set; }

        // O nome da espécie e a lista do top das capturas
        public Dictionary<string, List<PiscAtlas.Models.Models.Captura>> RankingsPorEspecie { get; set; } = new();

        public SelectList Pesqueiros { get; set; } = default!;

        public async Task OnGetAsync()
        {
            // Iniciar a query pelas capturas aprovadas e que têm peso registado
            var query = _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Utilizador)
                .Where(c => c.AprovadaPeloAdmin && c.Peso.HasValue)
                .AsQueryable();

            // Aplicar o filtro de Tempo
            var agora = DateTime.Now;
            if (TempoFiltro == "Mes")
            {
                query = query.Where(c => c.DataCaptura.Month == agora.Month && c.DataCaptura.Year == agora.Year);
            }
            else if (TempoFiltro == "Ano")
            {
                query = query.Where(c => c.DataCaptura.Year == agora.Year);
            }

            // Aplicar o filtro de Zona / Pesqueiro
            if (PesqueiroId.HasValue)
            {
                query = query.Where(c => c.PesqueiroId == PesqueiroId.Value);
            }

            // Ir buscar à Base de Dados
            var capturasFiltradas = await query.ToListAsync();

            // Agrupar por espécie e ficar apenas com o Top 10 de cada uma
            RankingsPorEspecie = capturasFiltradas
                .GroupBy(c => c.Especie?.Nome ?? "Desconhecida")
                .OrderBy(g => g.Key) // Ordenar alfabeticamente pelo nome do peixe
                .ToDictionary(
                    g => g.Key,
                    g => g.OrderByDescending(c => c.Peso).Take(10).ToList() // Top 10 mais pesados
                );

            // Preparar a lista de zonas para a dropdown do filtro
            Pesqueiros = new SelectList(await _context.Pesqueiros.OrderBy(p => p.Nome).ToListAsync(), "PesqueiroId", "Nome");
        }
    }
}