using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Evento
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

        public PiscAtlas.Models.Models.Evento EventoItem { get; set; } = default!;
        public bool JaInscrito { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            EventoItem = evento;

            // Verifica se o utilizador atual já está inscrito, validando pelo Email (como no teu controller)
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                JaInscrito = evento.Inscricoes?.Any(i => i.PescadorEmail == user!.Email) ?? false;
            }

            return Page();
        }

        // POST handler para a Inscrição
        public async Task<IActionResult> OnPostInscreverAsync(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Impede inscrição dupla (validação pelo Email)
            bool jaInscrito = evento.Inscricoes?.Any(i => i.PescadorEmail == user.Email) ?? false;
            if (jaInscrito)
            {
                TempData["Erro"] = "Já está inscrito neste evento.";
                return RedirectToPage(new { id });
            }

            // Impede inscrição em evento que já começou
            if (evento.DataInicio < DateTime.Now)
            {
                TempData["Erro"] = "Não é possível inscrever-se num evento já terminado.";
                return RedirectToPage(new { id });
            }

            // Criação exata da Inscrição baseada na tua Model
            var inscricao = new PiscAtlas.Models.Models.Inscricao
            {
                EventoId = evento.EventoId,
                PescadorEmail = user.Email!,
                PescadorNome = user.NomeCompleto,
                EstadoPagamento = PiscAtlas.Models.Models.EstadoPagamento.Pendente,
                ValorPago = evento.PrecoInscricao
            };

            _context.Inscricoes.Add(inscricao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Inscrição realizada com sucesso! Aguarda confirmação de pagamento.";
            return RedirectToPage(new { id });
        }
    }
}