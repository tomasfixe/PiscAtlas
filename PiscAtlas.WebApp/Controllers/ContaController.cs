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
                // 1. Define a imagem padrão (Caminho baseado na tua pasta wwwroot/images/Default.jpg)
                string fotoUrl = "/images/Default.jpg";

                // 2. Só entra aqui se o utilizador realmente tiver selecionado um ficheiro
                if (model.FotoFile != null && model.FotoFile.Length > 0)
                {
                    // Gera um nome único para não haver ficheiros repetidos
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.FotoFile.FileName);

                    // Define o caminho da pasta de uploads
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/perfis");

                    // Cria a pasta automaticamente se ela ainda não existir
                    if (!Directory.Exists(uploadPath))
                    {
                        Directory.CreateDirectory(uploadPath);
                    }

                    var filePath = Path.Combine(uploadPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await model.FotoFile.CopyToAsync(stream);
                    }

                    // Atualiza a fotoUrl para o novo ficheiro carregado
                    fotoUrl = "/uploads/perfis/" + fileName;
                }

                var user = new Utilizador
                {
                    UserName = model.Email,
                    Email = model.Email,
                    PrimeiroNome = model.PrimeiroNome,
                    UltimoNome = model.UltimoNome,
                    NomeUtilizador = model.NomeUtilizador,
                    FotografiaPerfilUrl = fotoUrl // Aqui grava ou a Default.jpg ou a nova foto
                };

                var result = await _userManager.CreateAsync(user, model.Password);

                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

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