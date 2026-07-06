using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Conta
{
    [Authorize]
    public class PerfilModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;

        public PerfilModel(UserManager<Utilizador> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public Utilizador PerfilUser { get; set; } = default!;
        public List<PiscAtlas.Models.Models.Captura> Capturas { get; set; } = new();

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            PerfilUser = user;

            Capturas = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Where(c => c.UtilizadorId == user.Id)
                .OrderByDescending(c => c.DataCaptura)
                .Take(6)
                .ToListAsync();

            return Page();
        }
    }
}