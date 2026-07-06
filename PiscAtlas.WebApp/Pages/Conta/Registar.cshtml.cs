using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Conta
{
    public class RegistarModel : PageModel
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public RegistarModel(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        [BindProperty]
        public RegistarInputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                string fotoUrl = "/images/Default.jpg";

                if (Input.FotoFile != null && Input.FotoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(Input.FotoFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/perfis");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await Input.FotoFile.CopyToAsync(stream);

                    fotoUrl = "/uploads/perfis/" + fileName;
                }

                var user = new Utilizador
                {
                    UserName = Input.Email,
                    Email = Input.Email,
                    PrimeiroNome = Input.PrimeiroNome,
                    UltimoNome = Input.UltimoNome,
                    NomeUtilizador = Input.NomeUtilizador,
                    FotografiaPerfilUrl = fotoUrl
                };

                var result = await _userManager.CreateAsync(user, Input.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return LocalRedirect("~/");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
            }
            return Page();
        }

        public class RegistarInputModel
        {
            [Required(ErrorMessage = "O Primeiro Nome é obrigatório.")]
            [Display(Name = "Primeiro Nome")]
            public string PrimeiroNome { get; set; } = string.Empty;

            [Required(ErrorMessage = "O Último Nome é obrigatório.")]
            [Display(Name = "Último Nome")]
            public string UltimoNome { get; set; } = string.Empty;

            [Required(ErrorMessage = "O Nome de Utilizador é obrigatório.")]
            [Display(Name = "Nome de Utilizador")]
            public string NomeUtilizador { get; set; } = string.Empty;

            [Required(ErrorMessage = "O Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-passe")]
            public string Password { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Palavra-passe")]
            [Compare("Password", ErrorMessage = "As palavras-passe não coincidem.")]
            public string ConfirmPassword { get; set; } = string.Empty;

            [Display(Name = "Fotografia de Perfil")]
            public IFormFile? FotoFile { get; set; }
        }
    }
}