using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using PiscAtlas.WebApp.Hubs;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Captura
{
    [Authorize]
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IHubContext<NotificacaoHub> _hubContext;

        public CriarModel(ApplicationDbContext context, UserManager<Utilizador> userManager, IHubContext<NotificacaoHub> hubContext)
        {
            _context = context;
            _userManager = userManager;
            _hubContext = hubContext;
        }

        [BindProperty]
        public CapturaInputModel Input { get; set; } = new();

        public SelectList Especies { get; set; } = default!;
        public SelectList Pesqueiros { get; set; } = default!;

        public async Task OnGetAsync()
        {
            await PopularSelectLists();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.FotoFile == null || Input.FotoFile.Length == 0)
            {
                ModelState.AddModelError("Input.FotoFile", "A fotografia é obrigatória.");
            }

            if (!ModelState.IsValid)
            {
                await PopularSelectLists();
                return Page();
            }

            var user = await _userManager.GetUserAsync(User);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.FotoFile!.FileName);
            var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/capturas");
            if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

            using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
            {
                await Input.FotoFile.CopyToAsync(stream);
            }

            var novaCaptura = new PiscAtlas.Models.Models.Captura
            {
                EspecieId = Input.EspecieId,
                PesqueiroId = Input.PesqueiroId,
                Peso = Input.Peso,
                Tamanho = Input.Tamanho,
                Notas = Input.Notas ?? "",
                Latitude = Input.Latitude ?? 0,
                Longitude = Input.Longitude ?? 0,
                UtilizadorId = user!.Id,
                FotografiaUrl = "/uploads/capturas/" + fileName,
                PossuiProvasVisuais = true,
                AprovadaPeloAdmin = false,
                DataCaptura = DateTime.Now
            };

            _context.Capturas.Add(novaCaptura);
            await _context.SaveChangesAsync();

            var especieInfo = await _context.Especies.FindAsync(Input.EspecieId);
            var pesqueiroInfo = await _context.Pesqueiros.FindAsync(Input.PesqueiroId);

            string mensagem = $"Nova captura! {user.NomeCompleto} acabou de registar um {especieInfo?.Nome} em {pesqueiroInfo?.Nome}.";
            await _hubContext.Clients.All.SendAsync("ReceberNotificacao", mensagem);

            TempData["Sucesso"] = "Captura registada! Fica a aguardar aprovação se introduziu peso/tamanho.";
            return RedirectToPage("./Index");
        }

        private async Task PopularSelectLists()
        {
            Especies = new SelectList(await _context.Especies.OrderBy(e => e.Nome).ToListAsync(), "EspecieId", "Nome");
            Pesqueiros = new SelectList(await _context.Pesqueiros.OrderBy(p => p.Nome).ToListAsync(), "PesqueiroId", "Nome");
        }

        public class CapturaInputModel
        {
            [Required(ErrorMessage = "Selecione uma espécie.")]
            public int EspecieId { get; set; }

            [Required(ErrorMessage = "Selecione um pesqueiro.")]
            public int PesqueiroId { get; set; }

            public double? Peso { get; set; }
            public double? Tamanho { get; set; }
            public string? Notas { get; set; }
            public double? Latitude { get; set; }
            public double? Longitude { get; set; }

            [Display(Name = "Fotografia")]
            public IFormFile? FotoFile { get; set; }
        }
    }
}