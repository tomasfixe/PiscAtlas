using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    public class DenunciaController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public DenunciaController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // POST: Denuncia/Criar — formulário inline (a partir da card de captura)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Criar(DenunciaViewModel model)
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var captura = await _context.Capturas.FindAsync(model.CapturaId);

                if (captura == null) return NotFound();

                // Impede auto-denúncia
                if (captura.UtilizadorId == user!.Id)
                {
                    TempData["Erro"] = "Não pode denunciar a sua própria captura.";
                    return RedirectToAction("Index", "Home");
                }

                // Impede denúncia duplicada do mesmo utilizador
                bool jaDenunciou = await _context.Denuncias
                    .AnyAsync(d => d.CapturaId == model.CapturaId && d.DenuncianteEmail == user.Email);

                if (jaDenunciou)
                {
                    TempData["Erro"] = "Já denunciou esta captura anteriormente.";
                    return RedirectToAction("Index", "Home");
                }

                var denuncia = new Denuncia
                {
                    CapturaId       = model.CapturaId,
                    DenuncianteEmail = user.Email!,
                    Motivo          = model.Motivo,
                    Estado          = EstadoDenuncia.Pendente,
                    DecisaoAdmin = ""
                };

                _context.Denuncias.Add(denuncia);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Denúncia submetida. Será analisada pela equipa PiscAtlas.";
            }
            else
            {
                TempData["Erro"] = "Preencha o motivo da denúncia.";
            }

            // Volta para a página de detalhes da captura se possível
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToAction("Index", "Home");
        }

        // GET: Denuncia/Detalhes/5 (apenas Admin)
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var denuncia = await _context.Denuncias
                .Include(d => d.Captura)
                    .ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura)
                    .ThenInclude(c => c.Especie)
                .FirstOrDefaultAsync(d => d.DenunciaId == id);

            if (denuncia == null) return NotFound();
            return View(denuncia);
        }

        // POST: Denuncia/Decidir — Admin resolve a denúncia
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Decidir(int denunciaId, string decisao, EstadoDenuncia estado)
        {
            var denuncia = await _context.Denuncias.FindAsync(denunciaId);
            if (denuncia == null) return NotFound();

            denuncia.DecisaoAdmin = decisao;
            denuncia.Estado       = estado;
            denuncia.DataDecisao  = DateTime.Now;

            // Se a denúncia for válida, marca a captura como não aprovada
            if (estado == EstadoDenuncia.Analisada_Valida)
            {
                var captura = await _context.Capturas.FindAsync(denuncia.CapturaId);
                if (captura != null)
                    captura.AprovadaPeloAdmin = false;
            }

            _context.Update(denuncia);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Decisão registada com sucesso.";
            return RedirectToAction("Denuncias", "Admin");
        }
    }

    public class DenunciaViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public int CapturaId { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "O motivo é obrigatório.")]
        [System.ComponentModel.DataAnnotations.StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;
    }
}
