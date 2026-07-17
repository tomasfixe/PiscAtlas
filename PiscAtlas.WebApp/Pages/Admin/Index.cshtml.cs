using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Admin
{
    // Acesso restrito a utilizadores com o cargo de Administrador
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

        // Propriedades para exibir estatísticas e listagens no dashboard
        public int TotalUtilizadores { get; set; }
        public int TotalCapturas { get; set; }
        public int TotalPesqueiros { get; set; }
        public int TotalDenunciasPendentes { get; set; }
        public List<PiscAtlas.Models.Models.Captura> CapturasRecentes { get; set; } = new();
        public List<PiscAtlas.Models.Models.Denuncia> DenunciasPendentes { get; set; } = new();

        public async Task OnGetAsync()
        {
            // Calcula contagens para os cartões de estatísticas
            TotalUtilizadores = await _userManager.Users.CountAsync();
            TotalCapturas = await _context.Capturas.CountAsync();
            TotalPesqueiros = await _context.Pesqueiros.CountAsync();
            TotalDenunciasPendentes = await _context.Denuncias.CountAsync(d => d.Estado == EstadoDenuncia.Pendente);

            // Lista as 5 capturas mais recentes
            CapturasRecentes = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Utilizador)
                .OrderByDescending(c => c.DataCaptura)
                .Take(5)
                .ToListAsync();

            // Lista as 5 denúncias mais recentes pendentes de análise
            DenunciasPendentes = await _context.Denuncias
                .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura).ThenInclude(c => c.Especie)
                .Where(d => d.Estado == EstadoDenuncia.Pendente)
                .OrderByDescending(d => d.DenunciaId)
                .Take(5)
                .ToListAsync();
        }

        // Método para aprovar manualmente uma captura pendente
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