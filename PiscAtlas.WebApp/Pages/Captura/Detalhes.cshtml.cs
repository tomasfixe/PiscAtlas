using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Captura
{
    public class DetalhesModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public DetalhesModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public PiscAtlas.Models.Models.Captura Captura { get; set; } = default!;

        // Variáveis para a UI de Interações
        public int TotalLikes { get; set; }
        public bool UtilizadorDeuLike { get; set; }
        public List<Interacao> Comentarios { get; set; } = new();

        [BindProperty]
        public string? NovoComentario { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var captura = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Utilizador)
                .Include(c => c.Interacoes) // Carrega as interações
                    .ThenInclude(i => i.Utilizador) // Carrega quem fez a interação
                .FirstOrDefaultAsync(m => m.CapturaId == id);

            if (captura == null) return NotFound();

            Captura = captura;

            // Contar likes e carregar comentários
            TotalLikes = captura.Interacoes.Count(i => i.Tipo == TipoInteracao.Gosto);
            Comentarios = captura.Interacoes
                .Where(i => i.Tipo == TipoInteracao.Comentario)
                .OrderBy(i => i.DataInteracao) // Os mais antigos primeiro
                .ToList();

            // Verificar se quem está a ver já deu Like
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                if (user != null)
                {
                    UtilizadorDeuLike = captura.Interacoes.Any(i => i.Tipo == TipoInteracao.Gosto && i.UtilizadorId == user.Id);
                }
            }

            return Page();
        }

        public async Task<IActionResult> OnPostLikeAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Obriga a login

            var likeExistente = await _context.Interacoes
                .FirstOrDefaultAsync(i => i.CapturaId == id && i.UtilizadorId == user.Id && i.Tipo == TipoInteracao.Gosto);

            if (likeExistente != null)
            {
                // Se já tem Like, removemos (Tirar Gosto)
                _context.Interacoes.Remove(likeExistente);
            }
            else
            {
                // Se não tem Like, adicionamos (Dar Gosto)
                _context.Interacoes.Add(new Interacao
                {
                    CapturaId = id,
                    UtilizadorId = user.Id,
                    Tipo = TipoInteracao.Gosto
                });
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id }); // Atualiza a página
        }

        public async Task<IActionResult> OnPostComentarAsync(int id)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge(); // Obriga a login

            if (!string.IsNullOrWhiteSpace(NovoComentario))
            {
                _context.Interacoes.Add(new Interacao
                {
                    CapturaId = id,
                    UtilizadorId = user.Id,
                    Tipo = TipoInteracao.Comentario,
                    Texto = NovoComentario
                });

                await _context.SaveChangesAsync();
            }

            return RedirectToPage(new { id });
        }
    }
}