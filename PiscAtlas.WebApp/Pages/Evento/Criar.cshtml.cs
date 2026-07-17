using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR; 
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.WebApp.Hubs; 
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Evento
{
    [Authorize(Roles = "Admin")]
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;
        private readonly IHubContext<NotificacaoHub> _hubContext; 

        
        public CriarModel(ApplicationDbContext context, IWebHostEnvironment env, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _env = env;
            _hubContext = hubContext;
        }

        [BindProperty]
        public EventoInputModel Input { get; set; } = new();

        public SelectList Especies { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await PopularSelectLists();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                await PopularSelectLists();
                return Page();
            }

            string fotoPath = string.Empty;
            if (Input.FotoFile != null)
            {
                var pasta = Path.Combine(_env.WebRootPath, "images", "eventos");
                Directory.CreateDirectory(pasta);
                var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(Input.FotoFile.FileName);
                var caminhoCompleto = Path.Combine(pasta, nomeFicheiro);
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await Input.FotoFile.CopyToAsync(stream);
                }
                fotoPath = "/images/eventos/" + nomeFicheiro;
            }

            var evento = new PiscAtlas.Models.Models.Evento
            {
                Nome = Input.Nome,
                Descricao = Input.Descricao ?? "",
                DataInicio = Input.DataInicio,
                DataFim = Input.DataFim,
                EspecieAlvoId = Input.EspecieAlvoId,
                PesoMinimo = Input.PesoMinimo,
                TamanhoMinimo = Input.TamanhoMinimo,
                PrecoInscricao = (decimal)(Input.PrecoInscricao ?? 0),
                FotografiaUrl = fotoPath
            };

            _context.Eventos.Add(evento);
            await _context.SaveChangesAsync();

            // DISPARAR NOTIFICAÇÃO DE NOVO EVENTO
            await _hubContext.Clients.All.SendAsync(
                "ReceberNovidade",
                "Evento",
                "Novo Evento Criado!",
                $"O evento '{evento.Nome}' já se encontra disponível."
            );

            TempData["Sucesso"] = "Evento criado com sucesso!";
            return RedirectToPage("./Index");
        }

        private async Task PopularSelectLists()
        {
            Especies = new SelectList(await _context.Especies.OrderBy(e => e.Nome).ToListAsync(), "EspecieId", "Nome");
        }

        public class EventoInputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório.")]
            public string Nome { get; set; } = string.Empty;
            public string? Descricao { get; set; }
            [Required(ErrorMessage = "A data de início é obrigatória.")]
            public DateTime DataInicio { get; set; } = DateTime.Now;
            [Required(ErrorMessage = "A data de fim é obrigatória.")]
            public DateTime DataFim { get; set; } = DateTime.Now.AddDays(1);
            [Required(ErrorMessage = "Selecione a espécie-alvo.")]
            public int EspecieAlvoId { get; set; }
            public double? PesoMinimo { get; set; }
            public double? TamanhoMinimo { get; set; }
            public double? PrecoInscricao { get; set; }
            public IFormFile? FotoFile { get; set; }
        }
    }
}