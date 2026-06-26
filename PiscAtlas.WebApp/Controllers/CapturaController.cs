using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    [Authorize]
    public class CapturaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public CapturaController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Captura/Index — lista as capturas do utilizador autenticado
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);
            var capturas = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Where(c => c.UtilizadorId == userId)
                .OrderByDescending(c => c.DataCaptura)
                .ToListAsync();

            return View(capturas);
        }

        // GET: Captura/Criar
        public async Task<IActionResult> Criar()
        {
            await PopularSelectLists();
            return View();
        }

        // POST: Captura/Criar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Criar(CapturaViewModel model)
        {
            if (ModelState.IsValid)
            {
                string fotografiaUrl = "/images/Default.jpg";

                if (model.FotoFile != null && model.FotoFile.Length > 0)
                {
                    var extensaoPermitida = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
                    var ext = Path.GetExtension(model.FotoFile.FileName).ToLowerInvariant();
                    if (!extensaoPermitida.Contains(ext))
                    {
                        ModelState.AddModelError("FotoFile", "Apenas são aceites imagens JPG, PNG, GIF ou WebP.");
                        await PopularSelectLists();
                        return View(model);
                    }

                    var fileName = Guid.NewGuid().ToString() + ext;
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/capturas");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FotoFile.CopyToAsync(stream);
                    }
                    fotografiaUrl = "/uploads/capturas/" + fileName;
                }

                var userId = _userManager.GetUserId(User);
                var captura = new Captura
                {
                    EspecieId     = model.EspecieId,
                    PesqueiroId   = model.PesqueiroId,
                    FotografiaUrl = fotografiaUrl,
                    Peso          = model.Peso,
                    Tamanho       = model.Tamanho,
                    Notas         = model.Notas ?? "",
                    Latitude      = model.Latitude ?? 0,
                    Longitude     = model.Longitude ?? 0,
                    PossuiProvasVisuais = model.FotoFile != null && model.FotoFile.Length > 0,
                    UtilizadorId  = userId!,
                    DataCaptura   = DateTime.Now
                };

                _context.Capturas.Add(captura);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Captura registada com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await PopularSelectLists();
            return View(model);
        }

        // GET: Captura/Detalhes/5
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var captura = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Utilizador)
                .Include(c => c.Denuncias)
                .FirstOrDefaultAsync(c => c.CapturaId == id);

            if (captura == null) return NotFound();

            return View(captura);
        }

        // GET: Captura/Editar/5
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas.FindAsync(id);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            var model = new CapturaViewModel
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
            ViewBag.FotoAtual = captura.FotografiaUrl;
            ViewBag.CapturaId = captura.CapturaId;
            return View(model);
        }

        // POST: Captura/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(int id, CapturaViewModel model)
        {
            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas.FindAsync(id);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            if (ModelState.IsValid)
            {
                if (model.FotoFile != null && model.FotoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.FotoFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/capturas");
                    if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);
                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await model.FotoFile.CopyToAsync(stream);
                    captura.FotografiaUrl = "/uploads/capturas/" + fileName;
                    captura.PossuiProvasVisuais = true;
                }

                captura.EspecieId   = model.EspecieId;
                captura.PesqueiroId = model.PesqueiroId;
                captura.Peso        = model.Peso;
                captura.Tamanho     = model.Tamanho;
                captura.Notas       = model.Notas ?? "";
                captura.Latitude    = model.Latitude ?? 0;
                captura.Longitude   = model.Longitude ?? 0;

                _context.Update(captura);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Captura atualizada!";
                return RedirectToAction(nameof(Index));
            }

            await PopularSelectLists();
            ViewBag.FotoAtual = captura.FotografiaUrl;
            ViewBag.CapturaId = id;
            return View(model);
        }

        // GET: Captura/Eliminar/5
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .FirstOrDefaultAsync(c => c.CapturaId == id);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            return View(captura);
        }

        // POST: Captura/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var userId = _userManager.GetUserId(User);
            var captura = await _context.Capturas.FindAsync(id);

            if (captura == null) return NotFound();
            if (captura.UtilizadorId != userId && !User.IsInRole("Admin"))
                return Forbid();

            _context.Capturas.Remove(captura);
            await _context.SaveChangesAsync();
            TempData["Sucesso"] = "Captura eliminada.";
            return RedirectToAction(nameof(Index));
        }

        private async Task PopularSelectLists()
        {
            ViewBag.Especies   = new SelectList(await _context.Especies.OrderBy(e => e.Nome).ToListAsync(), "EspecieId", "Nome");
            ViewBag.Pesqueiros = new SelectList(await _context.Pesqueiros.OrderBy(p => p.Nome).ToListAsync(), "PesqueiroId", "Nome");
        }
    }

    // ViewModel para criar/editar captura
    public class CapturaViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Selecione uma espécie.")]
        public int EspecieId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Selecione um pesqueiro.")]
        public int PesqueiroId { get; set; }

        public double? Peso { get; set; }
        public double? Tamanho { get; set; }
        public string? Notas { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Fotografia")]
        public IFormFile? FotoFile { get; set; }
    }
}
