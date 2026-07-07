using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Pesqueiro
{
    [Authorize(Roles = "Admin")]
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly IWebHostEnvironment _env;

        public CriarModel(ApplicationDbContext context, IWebHostEnvironment env)
        {
            _context = context;
            _env = env;
        }

        [BindProperty]
        public PesqueiroInputModel Input { get; set; } = new();

        public void OnGet() { }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid) return Page();

            string fotoPath = string.Empty;
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
                fotoPath = "/images/pesqueiros/" + nomeFicheiro;
            }

            var pesqueiro = new PiscAtlas.Models.Models.Pesqueiro
            {
                Nome = Input.Nome,
                Descricao = Input.Descricao ?? "",
                Latitude = Input.Latitude,
                Longitude = Input.Longitude,
                Tipo = Input.Tipo,
                FotografiaUrl = fotoPath
            };

            _context.Pesqueiros.Add(pesqueiro);
            await _context.SaveChangesAsync();
            return RedirectToPage("./Index");
        }

        public class PesqueiroInputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório.")]
            public string Nome { get; set; } = string.Empty;
            public string? Descricao { get; set; }
            public double Latitude { get; set; }
            public double Longitude { get; set; }
            public TipoPesqueiro Tipo { get; set; }
            public IFormFile? FotoFile { get; set; }
        }
    }
}