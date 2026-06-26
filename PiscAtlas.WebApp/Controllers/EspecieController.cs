using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    [Authorize]
    public class EspecieController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        // Injetamos a base de dados e o gestor de utilizadores no controlador
        public EspecieController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Ação que gera a página "Caderneta"
        public async Task<IActionResult> Index()
        {
            var userId = _userManager.GetUserId(User);

            //  Vai buscar todas as espécies
            var todasEspecies = await _context.Especies.OrderBy(e => e.Nome).ToListAsync();

            //  Vai buscar os IDs das espécies que este utilizador já apanhou
            var especiesCapturadasIds = await _context.Capturas
                .Where(c => c.UtilizadorId == userId)
                .Select(c => c.EspecieId)
                .Distinct() // Garante que não conta o duas vezes
                .ToListAsync();

            // 3. Empacota tudo para enviar para o Ecrã
            var vm = new CadernetaViewModel
            {
                TodasEspecies = todasEspecies,
                EspeciesCapturadasIds = especiesCapturadasIds
            };

            return View(vm);
        }
    }

    // ViewModel para transportar os dados da Caderneta
    public class CadernetaViewModel
    {
        public List<Especie> TodasEspecies { get; set; } = new();
        public List<int> EspeciesCapturadasIds { get; set; } = new();

        public int TotalCapturadas => EspeciesCapturadasIds.Count;
        public int TotalEspecies => TodasEspecies.Count;
        public int PorFazer => TotalEspecies - TotalCapturadas;
        public int Percentagem => TotalEspecies == 0 ? 0 : (int)Math.Round((double)TotalCapturadas / TotalEspecies * 100);
        public int Pontos => TotalCapturadas * 100; // 100 pontos por cada espécie diferente!
    }
}