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
                var result = await _signInManager.PasswordSignInAsync(
                    Input.Email, Input.Password, Input.LembrarMe, lockoutOnFailure: false);

                if (result.Succeeded)
                    return LocalRedirect("~/"); // Redireciona para a raiz (Home)

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Esta conta foi suspensa pela administração.");
                    return Page();
                }

                ModelState.AddModelError(string.Empty, "Email ou palavra-passe inválidos.");
            }
            return Page();
        }

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