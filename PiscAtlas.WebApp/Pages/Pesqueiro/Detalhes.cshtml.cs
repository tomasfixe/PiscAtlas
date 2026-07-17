using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Pesqueiro
{
    public class DetalhesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetalhesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PiscAtlas.Models.Models.Pesqueiro PesqueiroItem { get; set; } = default!;
        public List<PiscAtlas.Models.Models.Captura> TopCapturas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            // Carrega o pesqueiro e as suas capturas associadas com as espécies
            var pesqueiro = await _context.Pesqueiros
                .Include(p => p.Capturas!)
                    .ThenInclude(c => c.Especie)
                .FirstOrDefaultAsync(p => p.PesqueiroId == id);

            if (pesqueiro == null) return NotFound();

            PesqueiroItem = pesqueiro;

            // Obtém as 4 capturas mais pesadas aprovadas neste pesqueiro
            TopCapturas = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Utilizador)
                .Where(c => c.PesqueiroId == id && c.AprovadaPeloAdmin)
                .OrderByDescending(c => c.Peso)
                .Take(4)
                .ToListAsync();

            return Page();
        }
    }
}