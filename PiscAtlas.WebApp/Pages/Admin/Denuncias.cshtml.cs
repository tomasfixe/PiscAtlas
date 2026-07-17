using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Admin
{
    // Apenas administradores podem aceder a esta página
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
            // Carrega denúncias com os dados da captura relacionada
            var query = _context.Denuncias
                .Include(d => d.Captura).ThenInclude(c => c.Utilizador)
                .Include(d => d.Captura).ThenInclude(c => c.Especie)
                .AsQueryable();

            // Aplica filtro por estado caso seja fornecido
            if (EstadoFiltro.HasValue)
                query = query.Where(d => d.Estado == EstadoFiltro.Value);
            // Ordena pela mais recente
            Denuncias = await query.OrderByDescending(d => d.DenunciaId).ToListAsync();
        }
    }
}