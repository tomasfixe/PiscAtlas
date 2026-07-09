using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Pesquisa
{
    public class UtilizadoresModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public UtilizadoresModel(ApplicationDbContext context)
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
                // Pesquisa por Nome Completo ou por Nome de Utilizador (ignorando maiúsculas/minúsculas)
                Resultados = await _context.Users
                    .Where(u => u.NomeCompleto.Contains(Query) || u.NomeUtilizador.Contains(Query))
                    .OrderBy(u => u.NomeCompleto)
                    .Take(20) // Limita a 20 resultados para não pesar
                    .ToListAsync();
            }
        }
    }
}