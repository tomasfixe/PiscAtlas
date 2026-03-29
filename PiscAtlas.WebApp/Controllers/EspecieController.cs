using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;

namespace PiscAtlas.WebApp.Controllers
{
    public class EspecieController : Controller
    {
        private readonly ApplicationDbContext _context;

        // Injetamos a base de dados no controlador
        public EspecieController(ApplicationDbContext context)
        {
            _context = context;
        }

        // Ação que gera a página "Caderneta"
        public async Task<IActionResult> Index()
        {
            // Vai buscar todas as espécies à base de dados
            var especies = await _context.Especies.ToListAsync();
            return View(especies);
        }
    }
}