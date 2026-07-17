using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Captura
{
    [Authorize]
    public class EliminarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public EliminarModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public int CapturaId { get; set; }

        public PiscAtlas.Models.Models.Captura CapturaDetalhes { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            // Carrega a captura e dados associados para confirmação
            var captura = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .FirstOrDefaultAsync(c => c.CapturaId == id);

            if (captura == null) return NotFound();
            // Permissão: apenas o autor ou um administrador pode eliminar
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            CapturaDetalhes = captura;
            CapturaId = captura.CapturaId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas.FindAsync(CapturaId);

            if (captura == null) return NotFound();

            // Verificação de segurança antes de eliminar
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            // Remove a captura da base de dados
            _context.Capturas.Remove(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura eliminada.";
            return RedirectToPage("/Home/Index");
        }
    }
}