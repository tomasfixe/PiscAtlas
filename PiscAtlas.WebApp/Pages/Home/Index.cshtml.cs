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

        // Propriedade para saber qual aba está ativa 
        [BindProperty(SupportsGet = true)]
        public string Aba { get; set; } = "global";

        public async Task OnGetAsync()
        {
            // Iniciar a query base 
            var query = _context.Capturas
                .Include(c => c.Utilizador)
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Fotografias)
                .Include(c => c.Interacoes).ThenInclude(i => i.Utilizador)
                .Where(c => c.AprovadaPeloAdmin);

            // Lógica para utilizadores autenticados
            if (User.Identity?.IsAuthenticated == true)
            {
                var userId = _userManager.GetUserId(User);
                if (userId != null)
                {
                    // Se o utilizador escolheu a aba "A Seguir", aplicamos o filtro
                    if (Aba == "seguindo")
                    {
                        var seguidosIds = await _context.Seguidores
                            .Where(s => s.SeguidorId == userId)
                            .Select(s => s.SeguidoId)
                            .ToListAsync();

                        query = query.Where(c => seguidosIds.Contains(c.UtilizadorId));
                    }

                    // Carregar os gostos do utilizador atual
                    var gostos = await _context.Interacoes
                        .Where(i => i.UtilizadorId == userId && i.Tipo == TipoInteracao.Gosto)
                        .Select(i => i.CapturaId)
                        .ToListAsync();

                    CapturasGostadas = new HashSet<int>(gostos);
                }
            }
            else
            {
                // Garante que visitantes não logados veem sempre o global
                Aba = "global";
            }

            // Executar a query final ordenando pelas mais recentes
            Capturas = await query
                .OrderByDescending(c => c.DataCaptura)
                .Take(50)
                .ToListAsync();
        }

        // Adicionado o parâmetro 'aba' para não saltar para a aba global após um Like
        public async Task<IActionResult> OnPostLikeAsync(int capturaId, string aba = "global")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var likeExistente = await _context.Interacoes
                .FirstOrDefaultAsync(i => i.CapturaId == capturaId && i.UtilizadorId == user.Id && i.Tipo == TipoInteracao.Gosto);

            if (likeExistente != null) _context.Interacoes.Remove(likeExistente);
            else _context.Interacoes.Add(new Interacao { CapturaId = capturaId, UtilizadorId = user.Id, Tipo = TipoInteracao.Gosto });

            await _context.SaveChangesAsync();
            return Redirect(Url.Page("/Home/Index", new { aba = aba }) + "#captura-" + capturaId);
        }

        // Adicionado o parâmetro 'aba' para não saltar para a aba global após Comentar
        public async Task<IActionResult> OnPostComentarAsync(int capturaId, string comentarioTexto, string aba = "global")
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!string.IsNullOrWhiteSpace(comentarioTexto))
            {
                _context.Interacoes.Add(new Interacao { CapturaId = capturaId, UtilizadorId = user.Id, Tipo = TipoInteracao.Comentario, Texto = comentarioTexto });
                await _context.SaveChangesAsync();
            }
            return Redirect(Url.Page("/Home/Index", new { aba = aba }) + "#captura-" + capturaId);
        }
    }
}