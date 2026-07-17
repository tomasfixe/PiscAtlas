using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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

        public List<PiscAtlas.Models.Models.Especie> TodasEspecies { get; set; } = new();
        public List<int> EspeciesCapturadasIds { get; set; } = new();
        public Utilizador? CadernetaUser { get; set; }
        public bool IsProprio { get; set; }
        public bool AcessoNegado { get; set; } = false; 

        public int TotalCapturadas => EspeciesCapturadasIds.Count;
        public int TotalEspecies => TodasEspecies.Count;
        public int PorFazer => TotalEspecies - TotalCapturadas;
        public int Percentagem => TotalEspecies == 0 ? 0 : (int)Math.Round((double)TotalCapturadas / TotalEspecies * 100);
        public int Pontos => TotalCapturadas * 100;

        public async Task<IActionResult> OnGetAsync(string? userId)
        {
            var currentUserId = _userManager.GetUserId(User);
            string targetUserId = string.IsNullOrEmpty(userId) ? currentUserId : userId;

            if (string.IsNullOrEmpty(targetUserId)) return NotFound();

            CadernetaUser = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetUserId);
            if (CadernetaUser == null) return NotFound();

            IsProprio = (currentUserId == targetUserId);

            // FECHADURA DA CADERNETA PRIVADA:
            if (!IsProprio && CadernetaUser.CadernetaPrivada && !User.IsInRole("Admin"))
            {
                AcessoNegado = true;
                return Page(); // Para a execução aqui e mostra o aviso na página!
            }

            TodasEspecies = await _context.Especies.OrderBy(e => e.Nome).ToListAsync();
            EspeciesCapturadasIds = await _context.Capturas
                .Where(c => c.UtilizadorId == targetUserId && !c.FraudeConfirmada)
                .Select(c => c.EspecieId)
                .Distinct()
                .ToListAsync();

            return Page();
        }
    }
}