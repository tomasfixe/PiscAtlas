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
            .Include(c => c.Utilizador) // <--- Garante que vai buscar o nome do pescador!
            .Include(c => c.Especie)
            .Include(c => c.Pesqueiro)
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
    }

    // ViewModel específico para esta página (Agrupa os dados do Mapa e da Grelha)
    public class HomeViewModel
    {
        public IEnumerable<Pesqueiro> Pesqueiros { get; set; }
        public IEnumerable<Captura> Capturas { get; set; }
    }
}