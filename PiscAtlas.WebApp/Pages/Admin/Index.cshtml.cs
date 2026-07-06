using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public int TotalUtilizadores { get; set; }
        public int TotalCapturas { get; set; }
        public int TotalPesqueiros { get; set; }
        public int TotalDenunciasPendentes { get; set; }
        public List<Captura> CapturasRecentes { get; set; } = new();
        public List<Denuncia> DenunciasPendentes { get; set; } = new();

        public async Task OnGetAsync()
        {
            TotalUtilizadores = await _userManager.Users.CountAsync();
            TotalCapturas = await _context.Capturas.CountAsync();
            TotalPesqueiros = await _context.Pesqueiros.CountAsync();
            TotalDenunciasPendentes = await _context.Denuncias.CountAsync(d => d.Estado == EstadoDenuncia.Pendente);

            CapturasRecentes = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Utilizador)
                .OrderByDescending(c => c.DataCaptura)
                .Take(5)
                .ToListAsync();

            DenunciasPendentes = await _context.Denuncias
                .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura).ThenInclude(c => c.Especie)
                .Where(d => d.Estado == EstadoDenuncia.Pendente)
                .OrderByDescending(d => d.DenunciaId)
                .Take(5)
                .ToListAsync();
        }

        public async Task<IActionResult> OnPostAprovarCapturaAsync(int id)
        {
            var captura = await _context.Capturas.FindAsync(id);
            if (captura == null) return NotFound();

            captura.AprovadaPeloAdmin = true;
            _context.Update(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura aprovada!";
            return RedirectToPage();
        }
    }
}