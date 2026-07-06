using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Captura
{
    [Authorize]
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public EditarModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        [BindProperty]
        public CapturaEditModel Input { get; set; } = new();

        [BindProperty]
        public int CapturaId { get; set; }

        public string FotoAtual { get; set; } = string.Empty;

        public SelectList Especies { get; set; } = default!;
        public SelectList Pesqueiros { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas.FindAsync(id);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            CapturaId = captura.CapturaId;
            FotoAtual = captura.FotografiaUrl;

            Input = new CapturaEditModel
            {
                EspecieId = captura.EspecieId,
                PesqueiroId = captura.PesqueiroId,
                Peso = captura.Peso,
                Tamanho = captura.Tamanho,
                Notas = captura.Notas,
                Latitude = captura.Latitude,
                Longitude = captura.Longitude
            };

            await PopularSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas.FindAsync(CapturaId);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (!ModelState.IsValid)
            {
                FotoAtual = captura.FotografiaUrl;
                await PopularSelectLists();
                return Page();
            }

            if (Input.FotoFile != null && Input.FotoFile.Length > 0)
            {
                var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.FotoFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/capturas");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using (var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create))
                {
                    await Input.FotoFile.CopyToAsync(stream);
                }

                captura.FotografiaUrl = "/uploads/capturas/" + fileName;
                captura.PossuiProvasVisuais = true;
            }

            captura.EspecieId = Input.EspecieId;
            captura.PesqueiroId = Input.PesqueiroId;
            captura.Peso = Input.Peso;
            captura.Tamanho = Input.Tamanho;
            captura.Notas = Input.Notas ?? "";
            captura.Latitude = Input.Latitude ?? 0;
            captura.Longitude = Input.Longitude ?? 0;

            _context.Update(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura atualizada!";
            return RedirectToPage("./Index");
        }

        private async Task PopularSelectLists()
        {
            Especies = new SelectList(await _context.Especies.OrderBy(e => e.Nome).ToListAsync(), "EspecieId", "Nome");
            Pesqueiros = new SelectList(await _context.Pesqueiros.OrderBy(p => p.Nome).ToListAsync(), "PesqueiroId", "Nome");
        }

        public class CapturaEditModel
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

            [Display(Name = "Atualizar Fotografia")]
            public IFormFile? FotoFile { get; set; }
        }
    }
}