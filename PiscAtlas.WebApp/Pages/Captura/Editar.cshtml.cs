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
        private readonly IWebHostEnvironment _env;

        public EditarModel(ApplicationDbContext context, UserManager<Utilizador> userManager, IWebHostEnvironment env)
        {
            _context = context;
            _userManager = userManager;
            _env = env;
        }

        [BindProperty]
        public CapturaEditModel Input { get; set; } = new();

        [BindProperty]
        public int CapturaId { get; set; }

        public List<CapturaFotografia> FotografiasAtuais { get; set; } = new();

        public SelectList Especies { get; set; } = default!;
        public SelectList Pesqueiros { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas
                .Include(c => c.Fotografias)
                .FirstOrDefaultAsync(c => c.CapturaId == id);

            if (captura == null) return NotFound();

            // Permissão: apenas o autor ou um administrador pode editar
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            CapturaId = captura.CapturaId;
            FotografiasAtuais = captura.Fotografias.ToList();

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
            var captura = await _context.Capturas
                .Include(c => c.Fotografias)
                .FirstOrDefaultAsync(c => c.CapturaId == CapturaId);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (!ModelState.IsValid)
            {
                FotografiasAtuais = captura.Fotografias.ToList();
                await PopularSelectLists();
                return Page();
            }

            // 1. Apagar as fotos que o utilizador selecionou na checkbox
            if (Input.FotosParaRemover != null && Input.FotosParaRemover.Any())
            {
                var fotosApagar = captura.Fotografias.Where(f => Input.FotosParaRemover.Contains(f.Id)).ToList();
                foreach (var foto in fotosApagar)
                {
                    // Apaga fisicamente da pasta do servidor
                    var pathFisico = Path.Combine(_env.WebRootPath, foto.Url.TrimStart('/'));
                    if (System.IO.File.Exists(pathFisico)) System.IO.File.Delete(pathFisico);

                    _context.Remove(foto);
                }
            }

            // 2. Adicionar novas fotos à mesma captura
            if (Input.NovasFotos != null && Input.NovasFotos.Any())
            {
                var pasta = Path.Combine(_env.WebRootPath, "images", "capturas");
                if (!Directory.Exists(pasta)) Directory.CreateDirectory(pasta);

                foreach (var foto in Input.NovasFotos)
                {
                    if (foto.Length > 0)
                    {
                        var fileName = Guid.NewGuid().ToString() + Path.GetExtension(foto.FileName);
                        using (var stream = new FileStream(Path.Combine(pasta, fileName), FileMode.Create))
                        {
                            await foto.CopyToAsync(stream);
                        }

                        captura.Fotografias.Add(new CapturaFotografia { Url = "/images/capturas/" + fileName });
                        captura.PossuiProvasVisuais = true;
                    }
                }
            }

            // 3. Garantir que a Fotografia principal não fica corrompida
            var fotosRestantes = captura.Fotografias
                .Where(f => Input.FotosParaRemover == null || !Input.FotosParaRemover.Contains(f.Id))
                .ToList();

            if (fotosRestantes.Any())
            {
                captura.FotografiaUrl = fotosRestantes.First().Url;
            }
            else
            {
                captura.FotografiaUrl = "";
                captura.PossuiProvasVisuais = false;
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

            TempData["Sucesso"] = "Captura atualizada com sucesso!";
            return RedirectToPage("/Conta/Perfil");
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

            public List<int>? FotosParaRemover { get; set; }
            public List<IFormFile>? NovasFotos { get; set; }
        }
    }
}