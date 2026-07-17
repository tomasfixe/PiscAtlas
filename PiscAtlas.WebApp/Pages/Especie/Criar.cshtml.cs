using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.SignalR;
using PiscAtlas.Models;
using PiscAtlas.WebApp.Hubs;

namespace PiscAtlas.WebApp.Pages.Especie
{
    [Authorize(Roles = "Admin")]
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public CriarModel(ApplicationDbContext context, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _hubContext = hubContext;
        }

        [BindProperty]
        public PiscAtlas.Models.Models.Especie NovaEspecie { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Remover validações de campos que não pedimos no formulário
            ModelState.Remove("NovaEspecie.PesoRecordPt");
            ModelState.Remove("NovaEspecie.TamanhoRecordPt");
            ModelState.Remove("NovaEspecie.Capturas");
            ModelState.Remove("NovaEspecie.ImagemUrl");

            if (!ModelState.IsValid)
            {
                return Page();
            }

            NovaEspecie.Descricao = NovaEspecie.Descricao ?? "";
            NovaEspecie.ImagemUrl = "";
            NovaEspecie.PesoRecordPt = 0;
            NovaEspecie.TamanhoRecordPt = 0;

            _context.Especies.Add(NovaEspecie);
            await _context.SaveChangesAsync();

            // Signal R para notificar todos os clientes conectados sobre a nova espécie adicionada
            await _hubContext.Clients.All.SendAsync(
                "ReceberNovidade",
                "Especie",
                "Nova Espécie!",
                $"A espécie '{NovaEspecie.Nome}' foi adicionada."
            );

            TempData["Sucesso"] = "Nova espécie adicionada com sucesso à caderneta!";
            return RedirectToPage("./Index");
        }
    }
}