using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Admin
{
    [Authorize(Roles = "Admin")]
    public class DenunciasModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public DenunciasModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public List<PiscAtlas.Models.Models.Denuncia> Denuncias { get; set; } = new();
        public EstadoDenuncia? EstadoFiltro { get; set; }

        public async Task OnGetAsync(EstadoDenuncia? estado)
        {
            EstadoFiltro = estado;

            var query = _context.Denuncias
                .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura).ThenInclude(c => c.Especie)
                .AsQueryable();

            if (EstadoFiltro.HasValue)
                query = query.Where(d => d.Estado == EstadoFiltro.Value);

            Denuncias = await query.OrderByDescending(d => d.DenunciaId).ToListAsync();
        }
    }
}