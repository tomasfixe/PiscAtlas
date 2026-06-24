using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            ApplicationDbContext context,
            UserManager<Utilizador> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context     = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Admin/Index — Dashboard principal
        public async Task<IActionResult> Index()
        {
            var vm = new AdminDashboardViewModel
            {
                TotalUtilizadores   = await _userManager.Users.CountAsync(),
                TotalCapturas       = await _context.Capturas.CountAsync(),
                TotalPesqueiros     = await _context.Pesqueiros.CountAsync(),
                TotalDenunciasPendentes = await _context.Denuncias
                    .CountAsync(d => d.Estado == EstadoDenuncia.Pendente),
                CapturasRecentes    = await _context.Capturas
                    .Include(c => c.Especie)
                    .Include(c => c.Utilizador)
                    .OrderByDescending(c => c.DataCaptura)
                    .Take(5)
                    .ToListAsync(),
                DenunciasPendentes  = await _context.Denuncias
                    .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                    .Include(d => d.Captura).ThenInclude(c => c.Especie)
                    .Where(d => d.Estado == EstadoDenuncia.Pendente)
                    .OrderByDescending(d => d.DenunciaId)
                    .Take(5)
                    .ToListAsync()
            };

            return View(vm);
        }

        // GET: Admin/Capturas — lista todas as capturas para aprovação
        public async Task<IActionResult> Capturas(bool? pendentes)
        {
            var query = _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Utilizador)
                .AsQueryable();

            if (pendentes == true)
                query = query.Where(c => !c.AprovadaPeloAdmin && c.PossuiProvasVisuais);

            ViewBag.Pendentes = pendentes ?? false;
            var capturas = await query.OrderByDescending(c => c.DataCaptura).ToListAsync();
            return View(capturas);
        }

        // POST: Admin/AprovarCaptura/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AprovarCaptura(int id)
        {
            var captura = await _context.Capturas.FindAsync(id);
            if (captura == null) return NotFound();

            captura.AprovadaPeloAdmin = true;
            _context.Update(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura aprovada!";
            return RedirectToAction(nameof(Capturas));
        }

        // POST: Admin/RejeitarCaptura/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RejeitarCaptura(int id)
        {
            var captura = await _context.Capturas.FindAsync(id);
            if (captura == null) return NotFound();

            captura.AprovadaPeloAdmin = false;
            _context.Update(captura);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Captura rejeitada.";
            return RedirectToAction(nameof(Capturas));
        }

        // GET: Admin/Denuncias — lista todas as denúncias
        public async Task<IActionResult> Denuncias(EstadoDenuncia? estado)
        {
            var query = _context.Denuncias
                .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura).ThenInclude(c => c.Especie)
                .AsQueryable();

            if (estado.HasValue)
                query = query.Where(d => d.Estado == estado.Value);

            ViewBag.EstadoFiltro = estado;
            var denuncias = await query.OrderByDescending(d => d.DenunciaId).ToListAsync();
            return View(denuncias);
        }

        // GET: Admin/Utilizadores — lista todos os utilizadores
        public async Task<IActionResult> Utilizadores()
        {
            var users = await _userManager.Users
                .OrderBy(u => u.PrimeiroNome)
                .ToListAsync();

            // Para cada user, verifica se é Admin
            var admins = new HashSet<string>();
            var banidos = new HashSet<string>();
            foreach (var u in users)
            {
                if (await _userManager.IsInRoleAsync(u, "Admin"))
                    admins.Add(u.Id);

                if (await _userManager.IsLockedOutAsync(u))
                    banidos.Add(u.Id);
            }

            ViewBag.Admins = admins;
            ViewBag.Banidos = banidos;
            return View(users);
        }

        // POST: Admin/ToggleAdmin — promove/rebaixa um utilizador
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            bool isAdmin = await _userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
                await _userManager.RemoveFromRoleAsync(user, "Admin");
            else
                await _userManager.AddToRoleAsync(user, "Admin");

            TempData["Sucesso"] = isAdmin
                ? $"{user.NomeCompleto} removido do cargo Admin."
                : $"{user.NomeCompleto} promovido a Admin.";

            return RedirectToAction(nameof(Utilizadores));
        }

        // POST: Admin/BanirUtilizador — suspende a conta (impede login) sem a eliminar
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BanirUtilizador(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var idAtual = _userManager.GetUserId(User);
            if (user.Id == idAtual)
            {
                TempData["Erro"] = "Não pode banir a sua própria conta.";
                return RedirectToAction(nameof(Utilizadores));
            }

            if (!await _userManager.GetLockoutEnabledAsync(user))
                await _userManager.SetLockoutEnabledAsync(user, true);

            // Bane "para sempre" (100 anos)
            await _userManager.SetLockoutEndDateAsync(user, DateTimeOffset.UtcNow.AddYears(100));

            TempData["Sucesso"] = $"{user.NomeCompleto} foi suspenso e já não pode iniciar sessão.";
            return RedirectToAction(nameof(Utilizadores));
        }

        // POST: Admin/DesbanirUtilizador — remove a suspensão
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DesbanirUtilizador(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            await _userManager.SetLockoutEndDateAsync(user, null);

            TempData["Sucesso"] = $"A suspensão de {user.NomeCompleto} foi removida.";
            return RedirectToAction(nameof(Utilizadores));
        }

        // POST: Admin/EliminarUtilizador — elimina definitivamente a conta
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarUtilizador(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null) return NotFound();

            var idAtual = _userManager.GetUserId(User);
            if (user.Id == idAtual)
            {
                TempData["Erro"] = "Não pode eliminar a sua própria conta.";
                return RedirectToAction(nameof(Utilizadores));
            }

            var nome = user.NomeCompleto;
            var resultado = await _userManager.DeleteAsync(user);

            TempData["Sucesso"] = resultado.Succeeded
                ? $"A conta de {nome} foi eliminada."
                : $"Não foi possível eliminar a conta de {nome}.";

            return RedirectToAction(nameof(Utilizadores));
        }

        // GET: Admin/Especies — lista e gestão de espécies
        public async Task<IActionResult> Especies()
        {
            var especies = await _context.Especies.OrderBy(e => e.Nome).ToListAsync();
            return View(especies);
        }
    }

    // ViewModel do Dashboard
    public class AdminDashboardViewModel
    {
        public int TotalUtilizadores { get; set; }
        public int TotalCapturas { get; set; }
        public int TotalPesqueiros { get; set; }
        public int TotalDenunciasPendentes { get; set; }
        public List<Captura> CapturasRecentes { get; set; } = new();
        public List<Denuncia> DenunciasPendentes { get; set; } = new();
    }
}
