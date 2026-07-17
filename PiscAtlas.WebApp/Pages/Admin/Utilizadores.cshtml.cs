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
    public class UtilizadoresModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;

        public UtilizadoresModel(UserManager<Utilizador> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public List<Utilizador> Utilizadores { get; set; } = new();
        public HashSet<string> Admins { get; set; } = new();
        public HashSet<string> Banidos { get; set; } = new();

        public async Task OnGetAsync()
        {
            Utilizadores = await _userManager.Users
                .OrderBy(u => u.PrimeiroNome)
                .ToListAsync();

            foreach (var u in Utilizadores)
            {
                if (await _userManager.IsInRoleAsync(u, "Admin"))
                    Admins.Add(u.Id);

                if (await _userManager.IsLockedOutAsync(u))
                    Banidos.Add(u.Id);
            }
        }

        public async Task<IActionResult> OnPostToggleAdminAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "Admin");

            TempData["Sucesso"] = isAdmin
                ? $"{user.NomeCompleto} removido do cargo Admin."
                : $"{user.NomeCompleto} promovido a Admin.";

            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostBanirUtilizadorAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var idAtual = _userManager.GetUserId(User);
            if (user.Id == idAtual)
            {
                TempData["Erro"] = "Não pode banir a sua própria conta.";
                return RedirectToPage();
            }

            if (!await _userManager.GetLockoutEnabledAsync(user))
                await _userManager.SetLockoutEnabledAsync(user, true);

            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            TempData["Sucesso"] = $"{user.NomeCompleto} foi suspenso e já não pode iniciar sessão.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostDesbanirUtilizadorAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["Sucesso"] = $"A suspensão de {user.NomeCompleto} foi removida.";
            return RedirectToPage();
        }

        public async Task<IActionResult> OnPostEliminarUtilizadorAsync(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            // 1. Apagar todas as relações de Seguidores
            var seguidores = await _context.Seguidores
                .Where(s => s.SeguidorId == userId || s.SeguidoId == userId)
                .ToListAsync();
            _context.Seguidores.RemoveRange(seguidores);

            // 2. Encontrar todas as Capturas do utilizador
            var capturas = await _context.Capturas
                .Where(c => c.UtilizadorId == userId)
                .ToListAsync();

            if (capturas.Any())
            {
                var capturasIds = capturas.Select(c => c.CapturaId).ToList();

                // 3. CORREÇÃO: Usar o Set<CapturaFotografia>() para a pesquisa e _context.RemoveRange para apagar
                var fotografias = await _context.Set<CapturaFotografia>()
                    .Where(f => capturasIds.Contains(f.CapturaId))
                    .ToListAsync();
                _context.RemoveRange(fotografias);

                // 4. CORREÇÃO: Usar o Set<Interacao>() pelo mesmo motivo (precaução)
                var interacoes = await _context.Set<Interacao>()
                    .Where(i => capturasIds.Contains(i.CapturaId) || i.UtilizadorId == userId)
                    .ToListAsync();
                _context.RemoveRange(interacoes);

                // 5. Finalmente, apagar as Capturas
                _context.Capturas.RemoveRange(capturas);
            }

            // Gravar todas estas eliminações na base de dados ANTES de apagar o utilizador
            await _context.SaveChangesAsync();

            // 6. Agora sim, eliminar o utilizador com segurança
            var resultado = await _userManager.DeleteAsync(user);

            if (resultado.Succeeded)
            {
                TempData["Sucesso"] = "Utilizador e todos os seus dados foram eliminados com sucesso.";
            }
            else
            {
                TempData["Erro"] = "Ocorreu um erro ao tentar eliminar a conta do utilizador.";
            }

            return RedirectToPage();
        }
    }
}