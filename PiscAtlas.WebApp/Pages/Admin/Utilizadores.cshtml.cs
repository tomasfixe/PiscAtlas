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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Ninguém mexe nos privilégios do admin principal
            if (user.Email == "admin@piscatlas.pt")
            {
                TempData["Erro"] = "Operação bloqueada: Não é possível alterar os privilégios do Administrador Principal.";
                return RedirectToPage();
            }

            // Não podes tirar os teus próprios privilégios
            if (currentUser.Id == userId)
            {
                TempData["Erro"] = "Operação bloqueada: Não pode retirar os seus próprios privilégios de administrador.";
                return RedirectToPage();
            }

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");

            // Apenas o admin principal pode despromover outros admins
            if (isAdmin && currentUser.Email != "admin@piscatlas.pt")
            {
                TempData["Erro"] = "Permissão negada: Apenas o Administrador Principal pode remover os privilégios de outros administradores.";
                return RedirectToPage();
            }

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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Não podes banir a ti próprio
            if (user.Id == currentUser.Id)
            {
                TempData["Erro"] = "Operação bloqueada: Não pode banir a sua própria conta.";
                return RedirectToPage();
            }

            // O admin principal não pode ser banido
            if (user.Email == "admin@piscatlas.pt")
            {
                TempData["Erro"] = "Operação bloqueada: Não é possível suspender o Administrador Principal.";
                return RedirectToPage();
            }

            // Apenas o admin principal pode banir outros admins
            bool isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isTargetAdmin && currentUser.Email != "admin@piscatlas.pt")
            {
                TempData["Erro"] = "Permissão negada: Apenas o Administrador Principal pode suspender outros administradores.";
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

            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            // Não podes apagar a tua própria conta
            if (currentUser.Id == userId)
            {
                TempData["Erro"] = "Operação bloqueada: Não pode eliminar a sua própria conta.";
                return RedirectToPage();
            }

            // A conta admin original NUNCA pode ser apagada
            if (user.Email == "admin@piscatlas.pt")
            {
                TempData["Erro"] = "Operação bloqueada: O Administrador Principal não pode ser eliminado do sistema.";
                return RedirectToPage();
            }

            // Apenas o admin original pode apagar outros administradores
            bool isTargetAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isTargetAdmin && currentUser.Email != "admin@piscatlas.pt")
            {
                TempData["Erro"] = "Permissão negada: Apenas o Administrador Principal pode eliminar contas de outros administradores.";
                return RedirectToPage();
            }

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

                // 3. Usar o Set<CapturaFotografia>()
                var fotografias = await _context.Set<CapturaFotografia>()
                    .Where(f => capturasIds.Contains(f.CapturaId))
                    .ToListAsync();
                _context.RemoveRange(fotografias);

                // 4. Usar o Set<Interacao>()
                var interacoes = await _context.Set<Interacao>()
                    .Where(i => capturasIds.Contains(i.CapturaId) || i.UtilizadorId == userId)
                    .ToListAsync();
                _context.RemoveRange(interacoes);

                // 5. Finalmente, apagar as Capturas
                _context.Capturas.RemoveRange(capturas);
            }

            await _context.SaveChangesAsync();

            // 6. Eliminar o utilizador final
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