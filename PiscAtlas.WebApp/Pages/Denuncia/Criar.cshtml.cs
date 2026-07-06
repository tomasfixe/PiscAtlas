using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Denuncia
{
    [Authorize]
    public class CriarModel : PageModel
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public CriarModel(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Estas propriedades ligam-se automaticamente aos campos do modal original
        [BindProperty]
        [Required]
        public int CapturaId { get; set; }

        [BindProperty]
        [Required(ErrorMessage = "O motivo é obrigatório.")]
        [StringLength(500, ErrorMessage = "Máximo 500 caracteres.")]
        public string Motivo { get; set; } = string.Empty;

        public void OnGet()
        {
            // Se alguém tentar aceder ao link /Denuncia/Criar diretamente pelo browser,
            // atiramos de volta para a página inicial porque as denúncias só se fazem via modal.
            Response.Redirect("/");
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                var user = await _userManager.GetUserAsync(User);
                var captura = await _context.Capturas.FindAsync(CapturaId);

                if (captura == null) return NotFound();

                if (captura.UtilizadorId == user!.Id)
                {
                    TempData["Erro"] = "Não pode denunciar a sua própria captura.";
                    return RedirectToPage("/Index");
                }

                bool jaDenunciou = await _context.Denuncias
                    .AnyAsync(d => d.CapturaId == CapturaId && d.DenuncianteEmail == user.Email);

                if (jaDenunciou)
                {
                    TempData["Erro"] = "Já denunciou esta captura anteriormente.";
                    return RedirectToPage("/Index");
                }

                var denuncia = new PiscAtlas.Models.Models.Denuncia
                {
                    CapturaId = CapturaId,
                    DenuncianteEmail = user.Email!,
                    Motivo = Motivo,
                    Estado = PiscAtlas.Models.Models.EstadoDenuncia.Pendente,
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

            // Volta para a página de detalhes da captura de onde veio
            string? referer = Request.Headers["Referer"].ToString();
            if (!string.IsNullOrEmpty(referer))
                return Redirect(referer);

            return RedirectToPage("/Index");
        }
    }
}