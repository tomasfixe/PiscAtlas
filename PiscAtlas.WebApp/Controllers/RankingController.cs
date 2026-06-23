using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    public class RankingController : Controller
    {
        private readonly ApplicationDbContext _context;

        public RankingController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: Ranking/Index — maiores capturas aprovadas, com filtro opcional por espécie
        public async Task<IActionResult> Index(int? especieId)
        {
            var query = _context.Capturas
                .Include(c => c.Especie)
                .Include(c => c.Pesqueiro)
                .Include(c => c.Utilizador)
                .Where(c => c.AprovadaPeloAdmin && c.Peso.HasValue)
                .AsQueryable();

            if (especieId.HasValue)
                query = query.Where(c => c.EspecieId == especieId.Value);

            var capturas = await query
                .OrderByDescending(c => c.Peso)
                .Take(50)
                .ToListAsync();

            ViewBag.Especies = new SelectList(
                await _context.Especies.OrderBy(e => e.Nome).ToListAsync(),
                "EspecieId", "Nome", especieId);
            ViewBag.EspecieSelecionada = especieId;

            return View(capturas);
        }
    }
}
