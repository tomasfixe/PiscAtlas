using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Especie
{
    [Authorize]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Propriedades da Caderneta (o teu antigo ViewModel)
        public List<PiscAtlas.Models.Models.Especie> TodasEspecies { get; set; } = new();
        public List<int> EspeciesCapturadasIds { get; set; } = new();

        public int TotalCapturadas => EspeciesCapturadasIds.Count;
        public int TotalEspecies => TodasEspecies.Count;
        public int PorFazer => TotalEspecies - TotalCapturadas;
        public int Percentagem => TotalEspecies == 0 ? 0 : (int)Math.Round((double)TotalCapturadas / TotalEspecies * 100);
        public int Pontos => TotalCapturadas * 100; // 100 pontos por cada espécie diferente

        public async Task OnGetAsync()
        {
            var userId = _userManager.GetUserId(User);

            TodasEspecies = await _context.Especies.OrderBy(e => e.Nome).ToListAsync();

            EspeciesCapturadasIds = await _context.Capturas
                .Where(c => c.UtilizadorId == userId)
                .Select(c => c.EspecieId)
                .Distinct()
                .ToListAsync();
        }
    }
}