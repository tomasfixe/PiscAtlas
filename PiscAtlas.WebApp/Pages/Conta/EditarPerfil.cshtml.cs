using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Conta
{
    [Authorize]
    public class EditarPerfilModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;

        public EditarPerfilModel(UserManager<Utilizador> userManager)
        {
            _userManager = userManager;
        }

        [BindProperty]
        public EditarPerfilInputModel Input { get; set; } = new();

        public string FotoAtual { get; set; } = string.Empty;
        public string Iniciais { get; set; } = string.Empty;

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            FotoAtual = user.FotografiaPerfilUrl ?? "";
            Iniciais = user.PrimeiroNome?.Substring(0, 1).ToUpper() ?? "";

            Input = new EditarPerfilInputModel
            {
                PrimeiroNome = user.PrimeiroNome,
                UltimoNome = user.UltimoNome,
                NomeUtilizador = user.NomeUtilizador
            };

            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (!ModelState.IsValid)
            {
                FotoAtual = user.FotografiaPerfilUrl ?? "";
                Iniciais = user.PrimeiroNome?.Substring(0, 1).ToUpper() ?? "";
                return Page();
            }

            if (Input.FotoFile != null && Input.FotoFile.Length > 0)
            {
                var fileName = Guid.NewGuid() + Path.GetExtension(Input.FotoFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/perfis");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                await Input.FotoFile.CopyToAsync(stream);
                user.FotografiaPerfilUrl = "/uploads/perfis/" + fileName;
            }

            user.PrimeiroNome = Input.PrimeiroNome;
            user.UltimoNome = Input.UltimoNome;
            user.NomeUtilizador = Input.NomeUtilizador;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToPage("./Perfil");
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            FotoAtual = user.FotografiaPerfilUrl ?? "";
            Iniciais = user.PrimeiroNome?.Substring(0, 1).ToUpper() ?? "";
            return Page();
        }

        public class EditarPerfilInputModel
        {
            [Required(ErrorMessage = "O Primeiro Nome é obrigatório.")]
            public string PrimeiroNome { get; set; } = string.Empty;

            [Required(ErrorMessage = "O Último Nome é obrigatório.")]
            public string UltimoNome { get; set; } = string.Empty;

            [Required(ErrorMessage = "O Nome de Utilizador é obrigatório.")]
            public string NomeUtilizador { get; set; } = string.Empty;

            public IFormFile? FotoFile { get; set; }
        }
    }
}