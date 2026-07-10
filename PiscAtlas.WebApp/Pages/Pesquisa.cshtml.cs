using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages
{
    public class PesquisaModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public PesquisaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Utilizador> Resultados { get; set; } = new();
        public string Query { get; set; } = string.Empty;

        public async Task OnGetAsync(string? q)
        {
            Query = q ?? string.Empty;

            if (!string.IsNullOrWhiteSpace(Query))
            {
                // Agora pesquisa corretamente nas colunas que existem na Base de Dados
                Resultados = await _context.Users
                    .Where(u => u.PrimeiroNome.Contains(Query) ||
                                u.UltimoNome.Contains(Query) ||
                                u.NomeUtilizador.Contains(Query))
                    .OrderBy(u => u.PrimeiroNome)
                    .Take(20)
                    .ToListAsync();
            }
        }
    }
}