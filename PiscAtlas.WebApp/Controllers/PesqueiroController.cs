using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    public class PesqueiroController : Controller
    {
        private readonly ApplicationDbContext _context;

        public PesqueiroController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Pesqueiro/Index — lista com filtro por tipo
        public async Task<IActionResult> Index(TipoPesqueiro? tipo)
        {
            var query = _context.Pesqueiros
                .Include(p => p.Capturas)
                .AsQueryable();

            if (tipo.HasValue)
                query = query.Where(p => p.Tipo == tipo.Value);

            ViewBag.TipoSelecionado = tipo;
            ViewBag.Tipos = Enum.GetValues<TipoPesqueiro>();

            var pesqueiros = await query.OrderBy(p => p.Nome).ToListAsync();
            return View(pesqueiros);
        }

        // GET: Pesqueiro/Detalhes/5
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var pesqueiro = await _context.Pesqueiros
                .Include(p => p.Capturas)
                    .ThenInclude(c => c.Especie)
                .Include(p => p.Capturas)
                    .ThenInclude(c => c.Utilizador)
                .FirstOrDefaultAsync(p => p.PesqueiroId == id);

            if (pesqueiro == null) return NotFound();

            // Top capturas para este pesqueiro (por peso)
            ViewBag.TopCapturas = pesqueiro.Capturas
                .Where(c => c.Peso.HasValue)
                .OrderByDescending(c => c.Peso)
                .Take(5)
                .ToList();

            return View(pesqueiro);
        }

        // --- Área Admin: Criar/Editar/Eliminar ---

        [Authorize(Roles = "Admin")]
        public IActionResult Criar()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(PesqueiroViewModel model)
        {
            if (ModelState.IsValid)
            {
                string fotoUrl = "/images/Default.jpg";

                if (model.FotoFile != null && model.FotoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.FotoFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pesqueiros");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                    await model.FotoFile.CopyToAsync(stream);
                    fotoUrl = "/uploads/pesqueiros/" + fileName;
                }

                var pesqueiro = new Pesqueiro
                {
                    Nome         = model.Nome,
                    Descricao    = model.Descricao ?? "",
                    Latitude     = model.Latitude,
                    Longitude    = model.Longitude,
                    Tipo         = model.Tipo,
                    FotografiaUrl = fotoUrl
                };

                _context.Pesqueiros.Add(pesqueiro);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Pesqueiro criado!";
                return RedirectToAction(nameof(Index));
            }
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);
            if (pesqueiro == null) return NotFound();

            var model = new PesqueiroViewModel
            {
                Nome      = pesqueiro.Nome,
                Descricao = pesqueiro.Descricao,
                Latitude  = pesqueiro.Latitude,
                Longitude = pesqueiro.Longitude,
                Tipo      = pesqueiro.Tipo
            };

            ViewBag.FotoAtual    = pesqueiro.FotografiaUrl;
            ViewBag.PesqueiroId  = pesqueiro.PesqueiroId;
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int id, PesqueiroViewModel model)
        {
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);
            if (pesqueiro == null) return NotFound();

            if (ModelState.IsValid)
            {
                if (model.FotoFile != null && model.FotoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid() + Path.GetExtension(model.FotoFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/pesqueiros");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                    await model.FotoFile.CopyToAsync(stream);
                    pesqueiro.FotografiaUrl = "/uploads/pesqueiros/" + fileName;
                }

                pesqueiro.Nome      = model.Nome;
                pesqueiro.Descricao = model.Descricao ?? "";
                pesqueiro.Latitude  = model.Latitude;
                pesqueiro.Longitude = model.Longitude;
                pesqueiro.Tipo      = model.Tipo;

                _context.Update(pesqueiro);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Pesqueiro atualizado!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.FotoAtual   = pesqueiro.FotografiaUrl;
            ViewBag.PesqueiroId = id;
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();
            var pesqueiro = await _context.Pesqueiros
                .Include(p => p.Capturas)
                .FirstOrDefaultAsync(p => p.PesqueiroId == id);
            if (pesqueiro == null) return NotFound();
            return View(pesqueiro);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);
            if (pesqueiro != null)
            {
                _context.Pesqueiros.Remove(pesqueiro);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Pesqueiro eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }
    }

    public class PesqueiroViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
        public TipoPesqueiro Tipo { get; set; }
        public IFormFile? FotoFile { get; set; }
    }
}
