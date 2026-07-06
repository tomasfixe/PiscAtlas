using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class UtilizadoresModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;

        public UtilizadoresModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
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

            var idAtual = _userManager.GetUserId(User);
            if (user.Id == idAtual)
            {
                TempData["Erro"] = "Não pode eliminar a sua própria conta.";
                return RedirectToPage();
            }

            var nome = user.NomeCompleto;
            var resultado = await _userManager.DeleteAsync(user);

            TempData["Sucesso"] = resultado.Succeeded
                ? $"A conta de {nome} foi eliminada."
                : $"Não foi possível eliminar a conta de {nome}.";

            return RedirectToPage();
        }
    }
}