using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using PiscAtlas.WebApp.ViewModels;

namespace PiscAtlas.WebApp.Controllers
{
    public class ContaController : Controller
    {
        private readonly UserManager<Utilizador> _userManager;
        private readonly SignInManager<Utilizador> _signInManager;
        private readonly ApplicationDbContext _context;

        public ContaController(
            UserManager<Utilizador> userManager,
            SignInManager<Utilizador> signInManager,
            ApplicationDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _context = context;
        }

        // --- LOGIN ---
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
                var result = await _signInManager.PasswordSignInAsync(
                    model.Email, model.Password, model.LembrarMe, lockoutOnFailure: false);

                if (result.Succeeded)
                    return RedirectToAction("Index", "Home");

                if (result.IsLockedOut)
                {
                    ModelState.AddModelError(string.Empty, "Esta conta foi suspensa pela administração.");
                    return View(model);
                }

                ModelState.AddModelError(string.Empty, "Email ou palavra-passe inválidos.");
            }
            return View(model);
        }

        // --- REGISTAR ---
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
                string fotoUrl = "/images/Default.jpg";

                if (model.FotoFile != null && model.FotoFile.Length > 0)
                {
                    var fileName = Guid.NewGuid().ToString() + Path.GetExtension(model.FotoFile.FileName);
                    var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/perfis");
                    if (!Directory.Exists(uploadPath))
                        Directory.CreateDirectory(uploadPath);

                    var filePath = Path.Combine(uploadPath, fileName);
                    using (var stream = new FileStream(filePath, FileMode.Create))
                        await model.FotoFile.CopyToAsync(stream);

                    fotoUrl = "/uploads/perfis/" + fileName;
                }

                var user = new Utilizador
                {
                    UserName         = model.Email,
                    Email            = model.Email,
                    PrimeiroNome     = model.PrimeiroNome,
                    UltimoNome       = model.UltimoNome,
                    NomeUtilizador   = model.NomeUtilizador,
                    FotografiaPerfilUrl = fotoUrl
                };

                var result = await _userManager.CreateAsync(user, model.Password);
                if (result.Succeeded)
                {
                    await _signInManager.SignInAsync(user, isPersistent: false);
                    return RedirectToAction("Index", "Home");
                }

                foreach (var error in result.Errors)
                    ModelState.AddModelError(string.Empty, error.Description);
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

        // --- PERFIL ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> Perfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var capturas = await _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Where(c => c.UtilizadorId == user.Id)
                .OrderByDescending(c => c.DataCaptura)
                .Take(6)
                .ToListAsync();

            var vm = new PerfilViewModel
            {
                User     = user,
                Capturas = capturas
            };

            return View(vm);
        }

        // --- EDITAR PERFIL ---
        [Authorize]
        [HttpGet]
        public async Task<IActionResult> EditarPerfil()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            var model = new EditarPerfilViewModel
            {
                PrimeiroNome   = user.PrimeiroNome,
                UltimoNome     = user.UltimoNome,
                NomeUtilizador = user.NomeUtilizador,
                FotoAtual      = user.FotografiaPerfilUrl
            };
            return View(model);
        }

        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditarPerfil(EditarPerfilViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            if (model.FotoFile != null && model.FotoFile.Length > 0)
            {
                var fileName   = Guid.NewGuid() + Path.GetExtension(model.FotoFile.FileName);
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot/uploads/perfis");
                if (!Directory.Exists(uploadPath)) Directory.CreateDirectory(uploadPath);

                using var stream = new FileStream(Path.Combine(uploadPath, fileName), FileMode.Create);
                await model.FotoFile.CopyToAsync(stream);
                user.FotografiaPerfilUrl = "/uploads/perfis/" + fileName;
            }

            user.PrimeiroNome   = model.PrimeiroNome;
            user.UltimoNome     = model.UltimoNome;
            user.NomeUtilizador = model.NomeUtilizador;

            var result = await _userManager.UpdateAsync(user);
            if (result.Succeeded)
            {
                TempData["Sucesso"] = "Perfil atualizado com sucesso!";
                return RedirectToAction(nameof(Perfil));
            }

            foreach (var error in result.Errors)
                ModelState.AddModelError(string.Empty, error.Description);

            model.FotoAtual = user.FotografiaPerfilUrl;
            return View(model);
        }
    }
}
