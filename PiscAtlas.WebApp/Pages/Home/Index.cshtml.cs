using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Home
{
    public class IndexModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public IndexModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public List<PiscAtlas.Models.Models.Captura> Capturas { get; set; } = new();
        public HashSet<int> CapturasGostadas { get; set; } = new();

        public async Task OnGetAsync()
        {
            Capturas = await _context.Capturas
                .Include(c => c.Utilizador)
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Fotografias)
                .Include(c => c.Interacoes).ThenInclude(i => i.Utilizador)
                .Where(c => c.AprovadaPeloAdmin)
                .OrderByDescending(c => c.DataCaptura)
                .Take(50)
                .ToListAsync();

            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    var gostos = await _context.Interacoes
                        .Where(i => i.UtilizadorId == userId && i.Tipo == TipoInteracao.Gosto)
                        .Select(i => i.CapturaId)
                        .ToListAsync();

                    CapturasGostadas = new HashSet<int>(gostos);
                }
            }
        }

        public async Task<IActionResult> OnPostLikeAsync(int capturaId)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var likeExistente = await _context.Interacoes
                .FirstOrDefaultAsync(i => i.CapturaId == capturaId && i.UtilizadorId == user.Id && i.Tipo == TipoInteracao.Gosto);

            if (likeExistente != null) _context.Interacoes.Remove(likeExistente);
            else _context.Interacoes.Add(new Interacao { CapturaId = capturaId, UtilizadorId = user.Id, Tipo = TipoInteracao.Gosto });

            await _context.SaveChangesAsync();
            return Redirect(Url.Page("/Home/Index") + "#captura-" + capturaId);
        }

        public async Task<IActionResult> OnPostComentarAsync(int capturaId, string comentarioTexto)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!string.IsNullOrWhiteSpace(comentarioTexto))
            {
                _context.Interacoes.Add(new Interacao { CapturaId = capturaId, UtilizadorId = user.Id, Tipo = TipoInteracao.Comentario, Texto = comentarioTexto });
                await _context.SaveChangesAsync();
            }
            return Redirect(Url.Page("/Home/Index") + "#captura-" + capturaId);
        }
    }
}