using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Conta
{
    // Removemos o [Authorize] para que os perfis possam ser públicos
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

        public int TotalSeguidores { get; set; }
        public int TotalASeguir { get; set; }
        public bool IsSeguindo { get; set; }
        public bool IsProprioPerfil { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            string targetId = id ?? string.Empty;

            // Se o ID não for fornecido na URL, assumimos que o utilizador quer ver o próprio perfil
            if (string.IsNullOrEmpty(targetId))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Challenge(); // Se não estiver logado e não passou ID, manda pro Login
                targetId = currentUser.Id;
            }

            // Vai buscar os dados do perfil (seja o próprio ou o de outro)
            var user = await _context.Users
                .Include(u => u.Seguidores)
                .Include(u => u.A_Seguir)
                .FirstOrDefaultAsync(u => u.Id == targetId);

            if (user == null) return NotFound();

            PerfilUser = user;
            TotalSeguidores = user.Seguidores?.Count ?? 0;
            TotalASeguir = user.A_Seguir?.Count ?? 0;

            // Puxa as capturas (se for o próprio, pode ver todas, se for outro, só as aprovadas)
            var queryCapturas = _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Where(c => c.UtilizadorId == targetId);

            Capturas = await queryCapturas
                .OrderByDescending(c => c.DataCaptura)
                .ToListAsync();

            // Verifica as permissões de quem está a ver
            if (User.Identity?.IsAuthenticated == true)
            {
                var viewer = await _userManager.GetUserAsync(User);
                if (viewer != null)
                {
                    IsProprioPerfil = viewer.Id == targetId;
                    IsSeguindo = user.Seguidores?.Any(s => s.SeguidorId == viewer.Id) ?? false;
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostToggleSeguirAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge(); // Obriga a estar logado para seguir

            if (currentUser.Id == id) return BadRequest("Não pode seguir-se a si próprio.");

            var targetUser = await _context.Users
                .Include(u => u.Seguidores)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null) return NotFound();

            var seguimentoExistente = targetUser.Seguidores?.FirstOrDefault(s => s.SeguidorId == currentUser.Id);

            if (seguimentoExistente != null)
            {
                // Já segue -> Remover (Deixar de seguir)
                _context.Seguidores.Remove(seguimentoExistente);
            }
            else
            {
                // Não segue -> Adicionar (Seguir)
                _context.Seguidores.Add(new Seguidor
                {
                    SeguidorId = currentUser.Id,
                    SeguidoId = id
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }
    }
}