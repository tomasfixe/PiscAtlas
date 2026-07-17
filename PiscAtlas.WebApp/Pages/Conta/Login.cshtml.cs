using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Conta
{
    public class LoginModel : PageModel
    {
        private readonly SignInManager<Utilizador> _signInManager;

        public LoginModel(SignInManager<Utilizador> signInManager)
        {
            _signInManager = signInManager;
        }

        [BindProperty]
        public LoginInputModel Input { get; set; } = new();

        public void OnGet()
        {
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (ModelState.IsValid)
            {
                // Tenta autenticar o utilizador com as credenciais fornecidas
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, Input.Password, Input.LembrarMe, lockoutOnFailure: false);

                // Autenticação bem-sucedida: redireciona para a página inicial
                if (result.Succeeded)
                    return LocalRedirect("~/"); // Redireciona para a raiz (Home)

                // Verifica se a conta está suspensa (banida)
                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Esta conta foi suspensa pela administração.");
                    return Page();
                }

                // Credenciais incorretas
                ModelState.AddModelError(string.Empty, "Email ou palavra-passe inválidos.");
            }
            return Page();
        }

        // Modelo de dados para o formulário de Login
        public class LoginInputModel
        {
            [Required(ErrorMessage = "O Email é obrigatório.")]
            [EmailAddress(ErrorMessage = "Email inválido.")]
            public string Email { get; set; } = string.Empty;

            [Required(ErrorMessage = "A palavra-passe é obrigatória.")]
            [DataType(DataType.Password)]
            public string Password { get; set; } = string.Empty;

            [Display(Name = "Lembrar-me")]
            public bool LembrarMe { get; set; }
        }
    }
}