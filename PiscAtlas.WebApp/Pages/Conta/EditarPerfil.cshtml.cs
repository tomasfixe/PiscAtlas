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

        // 1. Formulário de Perfil Público
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

        // 2. Formulário de Privacidade da Conta (NOVO!)
        [BindProperty]
        public InputPrivacidadeModel InputPrivacidade { get; set; } = new();

        public class InputPrivacidadeModel
        {
            [Display(Name = "Conta Privada")]
            public bool ContaPrivada { get; set; }

            [Display(Name = "Ocultar Lista de Seguidores / A Seguir")]
            public bool ListaSeguidoresPrivada { get; set; }

            [Display(Name = "Caderneta de Espécies Privada")]
            public bool CadernetaPrivada { get; set; }
        }

        // 3. Formulário de Password
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

        // 4. Formulário de Aparência & Alertas (NOVO!)
        [BindProperty]
        public InputAparenciaModel InputAparencia { get; set; } = new();

        public class InputAparenciaModel
        {
            [Display(Name = "Tema Visual")]
            public string TemaVisual { get; set; } = "Claro"; // Opções: Claro, Escuro, Sistema

            [Display(Name = "Notificações Pop-up em Tempo Real (SignalR)")]
            public bool AlertasSignalR { get; set; } = true;
        }

        public async Task<IActionResult> OnGetAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            UtilizadorAtual = user;

            // Carregar dados de Perfil
            InputPerfil = new InputPerfilModel
            {
                PrimeiroNome = user.PrimeiroNome ?? string.Empty,
                UltimoNome = user.UltimoNome ?? string.Empty,
                NomeUtilizador = user.UserName ?? string.Empty,
                Email = user.Email ?? string.Empty,
                Telefone = user.PhoneNumber,
                FotografiaPerfilUrl = user.FotografiaPerfilUrl
            };

            // AGORA LÊ AS PREFERÊNCIAS DIRETAMENTE DA BASE DE DADOS:
            InputPrivacidade = new InputPrivacidadeModel
            {
                ContaPrivada = user.ContaPrivada,
                ListaSeguidoresPrivada = user.ListaSeguidoresPrivada,
                CadernetaPrivada = user.CadernetaPrivada
            };

            // AGORA LÊ AS PREFERÊNCIAS DE APARÊNCIA DIRETAMENTE DA BASE DE DADOS:
            InputAparencia = new InputAparenciaModel
            {
                TemaVisual = user.TemaVisual ?? "Claro",
                AlertasSignalR = user.AlertasSignalR
            };

            return Page();
        }

        // HANDLER 1: GUARDAR PERFIL
        public async Task<IActionResult> OnPostPerfilAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid) { UtilizadorAtual = user; return Page(); }

            user.PrimeiroNome = InputPerfil.PrimeiroNome;
            user.UltimoNome = InputPerfil.UltimoNome;
            user.PhoneNumber = InputPerfil.Telefone;
            user.FotografiaPerfilUrl = InputPerfil.FotografiaPerfilUrl;

            if (user.UserName != InputPerfil.NomeUtilizador)
            {
                var res = await _userManager.SetUserNameAsync(user, InputPerfil.NomeUtilizador);
                if (!res.Succeeded) { foreach (var e in res.Errors) ModelState.AddModelError("", e.Description); UtilizadorAtual = user; return Page(); }
            }

            if (user.Email != InputPerfil.Email)
            {
                var res = await _userManager.SetEmailAsync(user, InputPerfil.Email);
                if (!res.Succeeded) { foreach (var e in res.Errors) ModelState.AddModelError("", e.Description); UtilizadorAtual = user; return Page(); }
            }

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                await _signInManager.RefreshSignInAsync(user);
                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToPage();
            }

            foreach (var e in result.Errors) ModelState.AddModelError("", e.Description);
            UtilizadorAtual = user;
            return Page();
        }

        // HANDLER 2: GUARDAR PRIVACIDADE (GRAVA NA BASE DE DADOS REAL)
        public async Task<IActionResult> OnPostPrivacidadeAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.ContaPrivada = InputPrivacidade.ContaPrivada;
            user.ListaSeguidoresPrivada = InputPrivacidade.ListaSeguidoresPrivada;
            user.CadernetaPrivada = InputPrivacidade.CadernetaPrivada;

            await _userManager.UpdateAsync(user);

            TempData["Sucesso"] = "Definições de privacidade guardadas com sucesso!";
            return RedirectToPage();
        }

        // HANDLER 3: GUARDAR PASSWORD
        public async Task<IActionResult> OnPostPasswordAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            if (!ModelState.IsValid) { UtilizadorAtual = user; return Page(); }

            var res = await _userManager.ChangePasswordAsync(user, InputPassword.PasswordAtual, InputPassword.NovaPassword);
            if (!res.Succeeded)
            {
                foreach (var e in res.Errors) ModelState.AddModelError("InputPassword." + e.Code, e.Description);
                UtilizadorAtual = user;
                return Page();
            }

            await _signInManager.RefreshSignInAsync(user);
            TempData["Sucesso"] = "Palavra-passe alterada com sucesso!";
            return RedirectToPage();
        }

        // HANDLER 4: GUARDAR APARÊNCIA (GRAVA NA BASE DE DADOS REAL)
        public async Task<IActionResult> OnPostAparenciaAsync()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            user.TemaVisual = InputAparencia.TemaVisual;
            user.AlertasSignalR = InputAparencia.AlertasSignalR;

            await _userManager.UpdateAsync(user);

            TempData["Sucesso"] = $"Preferências guardadas! O tema '{user.TemaVisual}' está agora ativo.";
            return RedirectToPage();
        }
    }
}