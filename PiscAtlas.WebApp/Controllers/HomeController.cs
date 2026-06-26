using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    public class HomeController : Controller
    {
        private readonly ApplicationDbContext _context;

        public HomeController(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            // 1. Buscar todos os pesqueiros para os Pins do Mapa
            var pesqueiros = await _context.Pesqueiros.ToListAsync();

            // 2. Buscar as últimas 50 capturas (incluindo dados do utilizador e espécie)
            var capturas = await _context.Capturas
            .Include(c => c.Utilizador)
            .Include(c => c.Especie)
            .Include(c => c.Pesqueiro)
            .Where(c => !c.FraudeConfirmada)
            .OrderByDescending(c => c.DataCaptura)
            .Take(50)
            .ToListAsync();

            // 3. Montar o "pacote" de dados para a View
            var model = new HomeViewModel
            {
                Pesqueiros = pesqueiros,
                Capturas = capturas
            };

            return View(model);
        }

 

        // Página de Autores / Créditos
        public IActionResult Autores()
        {
            return View();
        }

        // Página de Erros Customizada
        [Route("Home/Erro")]
        public IActionResult Erro(int? statusCode = null)
        {
            if (statusCode.HasValue && statusCode.Value == 404)
            {
                ViewData["MensagemErro"] = "O peixe escapou! A página que procura não existe ou foi movida.";
                ViewData["CodigoErro"] = "404";
            }
            else
            {
                ViewData["MensagemErro"] = "Ocorreu um problema nas nossas águas. Tente novamente mais tarde.";
                ViewData["CodigoErro"] = statusCode?.ToString() ?? "500";
            }

            return View();
        }
    }

    // ViewModel específico para esta página (Agrupa os dados do Mapa e da Grelha)
    public class HomeViewModel
    {
        public IEnumerable<Pesqueiro> Pesqueiros { get; set; } = new List<Pesqueiro>();
        public IEnumerable<Captura> Capturas { get; set; } = new List<Captura>();
    }
}