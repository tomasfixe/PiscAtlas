using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class CapturasModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public CapturasModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<Captura> Capturas { get; set; } = new();
        public bool Pendentes { get; set; }

        public async Task OnGetAsync(bool? pendentes)
        {
            Pendentes = pendentes ?? false;

            var query = _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Utilizador)
                .AsQueryable();

            if (Pendentes)
                query = query.Where(c => !c.AprovadaPeloAdmin && c.PossuiProvasVisuais);

            Capturas = await query.OrderByDescending(c => c.DataCaptura).ToListAsync();
        }

        public async Task<IActionResult> OnPostAprovarCapturaAsync(int id, bool? pendentes)
        {
            var captura = await _context.Capturas.FindAsync(id);
            if (captura == null) return NotFound();

            captura.AprovadaPeloAdmin = true;
            _context.Update(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura aprovada!";
            return RedirectToPage(new { pendentes });
        }

        public async Task<IActionResult> OnPostRejeitarCapturaAsync(int id, bool? pendentes)
        {
            var captura = await _context.Capturas.FindAsync(id);
            if (captura == null) return NotFound();

            captura.AprovadaPeloAdmin = false;
            _context.Update(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura rejeitada.";
            return RedirectToPage(new { pendentes });
        }
    }
}