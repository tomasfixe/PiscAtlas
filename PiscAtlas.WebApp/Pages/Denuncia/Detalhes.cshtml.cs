using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Denuncia
{
    [Authorize(Roles = "Admin")]
    public class DetalhesModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DetalhesModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PiscAtlas.Models.Models.Denuncia DenunciaItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var denuncia = await _context.Denuncias
                .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura).ThenInclude(c => c.Especie)
                .FirstOrDefaultAsync(d => d.DenunciaId == id);

            if (denuncia == null) return NotFound();

            DenunciaItem = denuncia;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync(int denunciaId, string decisao, EstadoDenuncia estado)
        {
            var denuncia = await _context.Denuncias.FindAsync(denunciaId);
            if (denuncia == null) return NotFound();

            denuncia.DecisaoAdmin = decisao;
            denuncia.Estado = estado;
            denuncia.DataDecisao = DateTime.Now;

            // Se a denúncia for válida, marca a captura como não aprovada
            if (estado == EstadoDenuncia.Analisada_Valida)
            {
                var captura = await _context.Capturas.FindAsync(denuncia.CapturaId);
                if (captura != null)
                    captura.AprovadaPeloAdmin = false;
            }

            _context.Update(denuncia);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Decisão registada com sucesso.";
            return RedirectToPage("/Admin/Denuncias");
        }
    }
}