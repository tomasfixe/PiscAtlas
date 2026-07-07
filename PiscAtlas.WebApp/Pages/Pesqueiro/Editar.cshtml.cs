using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Pesqueiro
{
    [Authorize(Roles = "Admin")]
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public EditarModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public int PesqueiroId { get; set; }

        [BindProperty]
        public CriarModel.PesqueiroInputModel Input { get; set; } = new();

        public string? FotoAtual { get; set; }

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);
            if (pesqueiro == null) return NotFound();

            PesqueiroId = pesqueiro.PesqueiroId;
            FotoAtual = pesqueiro.FotografiaUrl;
            Input = new CriarModel.PesqueiroInputModel
            {
                Nome = pesqueiro.Nome,
                Descricao = pesqueiro.Descricao,
                Latitude = pesqueiro.Latitude,
                Longitude = pesqueiro.Longitude,
                Tipo = pesqueiro.Tipo
            };
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            var pesqueiro = await _context.Pesqueiros.FindAsync(PesqueiroId);
            if (pesqueiro == null) return NotFound();

            if (Input.FotoFile != null)
            {
                var pasta = Path.Combine(_env.WebRootPath, "images", "pesqueiros");
                Directory.CreateDirectory(pasta);
                var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(Input.FotoFile.FileName);
                var caminhoCompleto = Path.Combine(pasta, nomeFicheiro);
                using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                {
                    await Input.FotoFile.CopyToAsync(stream);
                }
                pesqueiro.FotografiaUrl = "/images/pesqueiros/" + nomeFicheiro;
            }

            pesqueiro.Nome = Input.Nome;
            pesqueiro.Descricao = Input.Descricao ?? "";
            pesqueiro.Latitude = Input.Latitude;
            pesqueiro.Longitude = Input.Longitude;
            pesqueiro.Tipo = Input.Tipo;

            _context.Update(pesqueiro);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Detalhes", new { id = PesqueiroId });
        }
    }
}