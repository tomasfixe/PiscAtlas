using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Captura
{
    [Authorize]
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly IWebHostEnvironment _env;

        public CriarModel(ApplicationDbContext context, UserManager<Utilizador> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public CapturaInputModel Input { get; set; } = new();

        public SelectList EspeciesList { get; set; } = default!;
        public SelectList PesqueirosList { get; set; } = default!;

        public void OnGet()
        {
            CarregarListas();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                CarregarListas();
                return Page();
            }

            var utilizadorAtual = await _userManager.GetUserAsync(User);
            if (utilizadorAtual == null) return Challenge();

            var novaCaptura = new PiscAtlas.Models.Models.Captura
            {
                EspecieId = Input.EspecieId,
                PesqueiroId = Input.PesqueiroId,
                UtilizadorId = utilizadorAtual.Id,
                Peso = Input.Peso,
                Tamanho = Input.Comprimento,
                Descricao = Input.Descricao,
                DataCaptura = Input.DataCaptura,
                AprovadaPeloAdmin = !(Input.Peso.HasValue || Input.Comprimento.HasValue),
                FotografiaUrl = "" // Vai ser preenchido abaixo
            };

            // Processar múltiplas fotos
            if (Input.FotosFiles != null && Input.FotosFiles.Count > 0)
            {
                var pasta = Path.Combine(_env.WebRootPath, "images", "capturas");
                if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

                bool isPrimeiraFoto = true;

                foreach (var ficheiro in Input.FotosFiles)
                {
                    if (ficheiro.Length > 0)
                    {
                        var nomeFicheiro = Guid.NewGuid().ToString() + Path.GetExtension(ficheiro.FileName);
                        var caminhoCompleto = Path.Combine(pasta, nomeFicheiro);

                        using (var stream = new FileStream(caminhoCompleto, FileMode.Create))
                        {
                            await ficheiro.CopyToAsync(stream);
                        }

                        var urlFoto = "/images/capturas/" + nomeFicheiro;

                        // A primeira foto fica como foto principal da captura
                        if (isPrimeiraFoto)
                        {
                            novaCaptura.FotografiaUrl = urlFoto;
                            isPrimeiraFoto = false;
                        }

                        // Adicionar à nova tabela de múltiplas fotos
                        novaCaptura.Fotografias.Add(new CapturaFotografia
                        {
                            Url = urlFoto
                        });
                    }
                }
                novaCaptura.PossuiProvasVisuais = true;
            }

            _context.Capturas.Add(novaCaptura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura registada com sucesso!";
            return RedirectToPage("/Home/Index");
        }

        private void CarregarListas()
        {
            EspeciesList = new SelectList(_context.Especies.OrderBy(e => e.Nome), "EspecieId", "Nome");
            PesqueirosList = new SelectList(_context.Pesqueiros.OrderBy(p => p.Nome), "PesqueiroId", "Nome");
        }

        public class CapturaInputModel
        {
            [Required(ErrorMessage = "A espécie é obrigatória!")]
            public int EspecieId { get; set; }

            [Required(ErrorMessage = "O pesqueiro é obrigatório!")]
            public int PesqueiroId { get; set; }

            public double? Peso { get; set; }
            public double? Comprimento { get; set; }
            public string? Descricao { get; set; }

            [Required(ErrorMessage = "A data da captura é obrigatória!")]
            public DateTime DataCaptura { get; set; } = DateTime.Now;

            [Required(ErrorMessage = "Tem de enviar pelo menos uma fotografia.")]
            public List<IFormFile> FotosFiles { get; set; } = new();
        }
    }
}