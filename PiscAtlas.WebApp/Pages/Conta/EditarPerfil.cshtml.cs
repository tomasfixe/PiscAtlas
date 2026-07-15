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
        private readonly SignInManager<Utilizador> _signInManager;

        public EditarPerfilModel(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public Utilizador UtilizadorAtual { get; set; } = default!;

        // Formulário de Perfil
        [BindProperty]
        public InputPerfilModel InputPerfil { get; set; } = new();

        public class InputPerfilModel
        {
            [Required(ErrorMessage = "O primeiro nome é obrigatório.")]
            [Display(Name = "Primeiro Nome")]
            public string PrimeiroNome { get; set; } = string.Empty;

            [Required(ErrorMessage = "O último nome é obrigatório.")]
            [Display(Name = "Último Nome")]
            public string UltimoNome { get; set; } = string.Empty;

            [Display(Name = "Nome de Utilizador")]
            public string NomeUtilizador { get; set; } = string.Empty;

            [EmailAddress(ErrorMessage = "Email inválido.")]
            [Display(Name = "Email")]
            public string Email { get; set; } = string.Empty;

            [Phone(ErrorMessage = "Número de telefone inválido.")]
            [Display(Name = "Telefone")]
            public string? Telefone { get; set; }

            [Url(ErrorMessage = "Insira um URL válido para a imagem.")]
            [Display(Name = "URL da Fotografia de Perfil")]
            public string? FotografiaPerfilUrl { get; set; }
        }

        // Formulário de Password
        [BindProperty]
        public InputPasswordModel InputPassword { get; set; } = new();

        public class InputPasswordModel
        {
            [Required(ErrorMessage = "Insira a palavra-passe atual.")]
            [DataType(DataType.Password)]
            [Display(Name = "Palavra-passe Atual")]
            public string PasswordAtual { get; set; } = string.Empty;

            [Required(ErrorMessage = "Insira a nova palavra-passe.")]
            [StringLength(100, ErrorMessage = "A {0} deve ter pelo menos {2} caracteres.", MinimumLength = 6)]
            [DataType(DataType.Password)]
            [Display(Name = "Nova Palavra-passe")]
            public string NovaPassword { get; set; } = string.Empty;

            [DataType(DataType.Password)]
            [Display(Name = "Confirmar Nova Palavra-passe")]
            [Compare("NovaPassword", ErrorMessage = "A nova palavra-passe e a confirmação não coincidem.")]
            public string ConfirmarPassword { get; set; } = string.Empty;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            UtilizadorAtual = user;

            InputPerfil = new InputPerfilModel
            {
                PrimeiroNome = user.PrimeiroNome ?? string.Empty,
                UltimoNome = user.UltimoNome ?? string.Empty,
                NomeUtilizador = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Telefone = user.PhoneNumber,
                FotografiaPerfilUrl = user.FotografiaPerfilUrl
            };

            return Page();
        }

        public async Task<IActionResult> OnPostPerfilAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                UtilizadorAtual = user;
                return Page();
            }

            user.PrimeiroNome = InputPerfil.PrimeiroNome;
            user.UltimoNome = InputPerfil.UltimoNome;
            user.PhoneNumber = InputPerfil.Telefone;
            user.FotografiaPerfilUrl = InputPerfil.FotografiaPerfilUrl;

            // Mudar Username se foi alterado
            if (user.UserName != InputPerfil.NomeUtilizador)
            {
                var setUsernameResult = await _userManager.SetUserNameAsync(user, InputPerfil.NomeUtilizador);
                if (!setUsernameResult.Succeeded)
                {
                    foreach (var error in setUsernameResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                    UtilizadorAtual = user;
                    return Page();
                }
            }

            // Mudar Email se foi alterado
            if (user.Email != InputPerfil.Email)
            {
                var setEmailResult = await _userManager.SetEmailAsync(user, InputPerfil.Email);
                if (!setEmailResult.Succeeded)
                {
                    foreach (var error in setEmailResult.Errors) ModelState.AddModelError(string.Empty, error.Description);
                    UtilizadorAtual = user;
                    return Page();
                }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToPage();
            }

            foreach (var error in result.Errors) ModelState.AddModelError(string.Empty, error.Description);
            UtilizadorAtual = user;
            return Page();
        }

        public async Task<IActionResult> OnPostPasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid)
            {
                UtilizadorAtual = user;
                return Page();
            }

            var changePasswordResult = await _userManager.ChangePasswordAsync(user, InputPassword.PasswordAtual, InputPassword.NovaPassword);
            if (!changePasswordResult.Succeeded)
            {
                foreach (var error in changePasswordResult.Errors)
                {
                    ModelState.AddModelError("InputPassword." + error.Code, error.Description);
                }
                UtilizadorAtual = user;
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Sucesso"] = "Palavra-passe alterada com sucesso!";
            return RedirectToPage();
        }
    }
}