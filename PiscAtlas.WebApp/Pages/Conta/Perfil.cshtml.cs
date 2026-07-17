using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using PiscAtlas.WebApp.Hubs;

namespace PiscAtlas.WebApp.Pages.Conta
{
    public class PerfilModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public PerfilModel(UserManager<Utilizador> userManager, ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _userManager = userManager;
            _context = context;
            _hubContext = hubContext;
        }

        public Utilizador PerfilUser { get; set; } = default!;
        public List<PiscAtlas.Models.Models.Captura> Capturas { get; set; } = new();
        public List<Seguidor> PedidosPendentes { get; set; } = new();

        public int TotalSeguidores { get; set; }
        public int TotalASeguir { get; set; }
        public bool IsSeguindo { get; set; }
        public bool IsPendente { get; set; }
        public bool IsProprioPerfil { get; set; }

        public async Task<IActionResult> OnGetAsync(string? id)
        {
            string targetId = id ?? string.Empty;

            // Se não houver ID, assume o perfil do utilizador atual
            if (string.IsNullOrEmpty(targetId))
            {
                var currentUser = await _userManager.GetUserAsync(User);
                if (currentUser == null) return Challenge();
                targetId = currentUser.Id;
            }

            var user = await _context.Users
                .Include(u => u.Seguidores)
                .Include(u => u.A_Seguir)
                .FirstOrDefaultAsync(u => u.Id == targetId);

            if (user == null) return NotFound();

            PerfilUser = user;
            // Apenas conta seguidores que já foram aceites (não pendentes)
            TotalSeguidores = user.Seguidores?.Count(s => !s.Pendente) ?? 0;
            TotalASeguir = user.A_Seguir?.Count(s => !s.Pendente) ?? 0;

            if (User.Identity?.IsAuthenticated == true)
            {
                var viewer = await _userManager.GetUserAsync(User);
                if (viewer != null)
                {
                    IsProprioPerfil = viewer.Id == targetId;

                    var relacao = user.Seguidores?.FirstOrDefault(s => s.SeguidorId == viewer.Id);
                    if (relacao != null)
                    {
                        if (relacao.Pendente) IsPendente = true;
                        else IsSeguindo = true;
                    }

                    if (IsProprioPerfil)
                    {
                        PedidosPendentes = await _context.Seguidores
                            .Include(s => s.UtilizadorSeguidor)
                            .Where(s => s.SeguidoId == targetId && s.Pendente)
                            .ToListAsync();
                    }
                }
            }

            // Bloqueia a visualização das capturas se a conta for privada e não seguir
            if (user.ContaPrivada && !IsProprioPerfil && !IsSeguindo && !User.IsInRole("Admin"))
            {
                Capturas = new List<PiscAtlas.Models.Models.Captura>();
            }
            else
            {
                Capturas = await _context.Capturas
                    .Include(c => c.Especie)
                    .Include(c => c.Pesqueiro)
                    .Where(c => c.UtilizadorId == targetId)
                    .OrderByDescending(c => c.DataCaptura)
                    .ToListAsync();
            }

            return Page();
        }

        // Alterna entre seguir, deixar de seguir ou pedir para seguir
        public async Task<IActionResult> OnPostToggleSeguirAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();
            if (currentUser.Id == id) return BadRequest("Não pode seguir-se a si próprio.");

            var targetUser = await _context.Users
                .Include(u => u.Seguidores)
                .FirstOrDefaultAsync(u => u.Id == id);

            if (targetUser == null) return NotFound();

            var seguimentoExistente = targetUser.Seguidores?.FirstOrDefault(s => s.SeguidorId == currentUser.Id);

            if (seguimentoExistente != null)
            {
                _context.Seguidores.Remove(seguimentoExistente);
            }
            else
            {
                // Cria relação, pendente se o utilizador for privado
                var novoSeguidor = new Seguidor
                {
                    SeguidorId = currentUser.Id,
                    SeguidoId = id,
                    Pendente = targetUser.ContaPrivada
                };
                _context.Seguidores.Add(novoSeguidor);

                // Notifica o utilizador alvo via SignalR
                if (novoSeguidor.Pendente)
                {
                    await _hubContext.Clients.All.SendAsync("ReceberNovidade", "Utilizador", "Novo Pedido", $"{currentUser.NomeUtilizador} quer seguir-te!");
                }
                else
                {
                    await _hubContext.Clients.All.SendAsync("ReceberNovidade", "Utilizador", "Novo Seguidor", $"{currentUser.NomeUtilizador} começou a seguir-te!");
                }
            }

            await _context.SaveChangesAsync();
            return RedirectToPage(new { id });
        }

        // Aceita o pedido de follow
        public async Task<IActionResult> OnPostAceitarPedidoAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var relacao = await _context.Seguidores.FirstOrDefaultAsync(s => s.SeguidoId == currentUser.Id && s.SeguidorId == id);
            if (relacao != null)
            {
                relacao.Pendente = false;
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Pedido de seguimento aceite!";
            }
            return RedirectToPage();
        }

        // Rejeita o pedido
        public async Task<IActionResult> OnPostRejeitarPedidoAsync(string id)
        {
            var currentUser = await _userManager.GetUserAsync(User);
            if (currentUser == null) return Challenge();

            var relacao = await _context.Seguidores.FirstOrDefaultAsync(s => s.SeguidoId == currentUser.Id && s.SeguidorId == id);
            if (relacao != null)
            {
                _context.Seguidores.Remove(relacao);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage();
        }
    }
}