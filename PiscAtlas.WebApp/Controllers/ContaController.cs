using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using PiscAtlas.Models.Models;
using PiscAtlas.WebApp.ViewModels;

namespace PiscAtlas.WebApp.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;

        public ContaController(UserManager<Utilizador> userManager, SignInManager<Utilizador> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        // --- PÁGINA DE LOGIN ---
        [HttpGet]
        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Tenta fazer o login com o email (UserName) e a password
                var result = await _signInManager.PasswordSignInAsync(model.Email, model.Password, model.LembrarMe, lockoutOnFailure: false);

                if (result.Succeeded)
                {
                    return RedirectToAction("Index", "Home"); // Sucesso! Vai para o mapa.
                }

                ModelState.AddModelError(string.Empty, "Email ou palavra-passe inválidos.");
            }
            return View(model);
        }

        // --- PÁGINA DE REGISTO ---
        [HttpGet]
        public IActionResult Registar()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Registar(RegistarViewModel model)
        {
            if (ModelState.IsValid)
            {
                // Cria o objeto Utilizador com os dados do formulário
                var user = new Utilizador
                {
                    UserName = model.Email, // Usamos o Email como UserName de login
                    Email = model.Email,
                    PrimeiroNome = model.PrimeiroNome,
                    UltimoNome = model.UltimoNome,
                    NomeUtilizador = model.NomeUtilizador
                };

                // Pede à base de dados para criar a conta de forma segura
                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    // Se correu bem, inicia a sessão automaticamente
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                // Se houver erros (ex: email já existe), mostra-os no ecrã
                foreach (var error in result.Errors)
                {
                    ModelState.AddModelError(string.Empty, error.Description);
                }
            }
            return View(model);
        }

        // --- TERMINAR SESSÃO ---
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await _signInManager.SignOutAsync();
            return RedirectToAction("Index", "Home");
        }
    }
}