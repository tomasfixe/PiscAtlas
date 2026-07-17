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
    public class RedeModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public RedeModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        public Utilizador Alvo { get; set; } = default!;
        public string AbaAtiva { get; set; } = "seguidores";
        public bool IsProprio { get; set; }
        public bool AcessoNegadoPrivacidade { get; set; } = false;

        public List<Utilizador> ListaUtilizadores { get; set; } = new();

        public async Task<IActionResult> OnGetAsync(string? id, string? aba)
        {
            var currentUserId = _userManager.GetUserId(User);
            string targetId = string.IsNullOrEmpty(id) ? currentUserId : id;

            var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == targetId);
            if (user == null) return NotFound();

            Alvo = user;
            IsProprio = (currentUserId == targetId);
            if (!string.IsNullOrEmpty(aba)) AbaAtiva = aba;

            // VERIFICAÇÃO DE PRIVACIDADE DA LISTA DE SEGUIDORES:
            // Se for privado, bloqueia o acesso (a não ser que seja um Admin ou o próprio)
            if (!IsProprio && Alvo.ListaSeguidoresPrivada && !User.IsInRole("Admin"))
            {
                AcessoNegadoPrivacidade = true;
                return Page();
            }

            // CARREGAR A LISTA DA BASE DE DADOS DEPENDENDO DA ABA
            if (AbaAtiva == "seguidores")
            {
                // Mostra quem me segue (ignorando quem ainda está pendente, a não ser que seja o próprio a ver)
                var seguidores = await _context.Seguidores
                    .Include(s => s.UtilizadorSeguidor)
                    .Where(s => s.SeguidoId == targetId && (!s.Pendente || IsProprio))
                    .Select(s => s.UtilizadorSeguidor!)
                    .ToListAsync();

                ListaUtilizadores = seguidores;
            }
            else // Aba: A Seguir
            {
                // Mostra quem eu sigo (ignorando pedidos pendentes que ainda não foram aceites)
                var aSeguir = await _context.Seguidores
                    .Include(s => s.UtilizadorSeguido)
                    .Where(s => s.SeguidorId == targetId && !s.Pendente)
                    .Select(s => s.UtilizadorSeguido!)
                    .ToListAsync();

                ListaUtilizadores = aSeguir;
            }

            return Page();
        }
    }
}