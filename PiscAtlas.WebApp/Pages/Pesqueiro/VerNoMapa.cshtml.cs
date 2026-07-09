using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Pages.Pesqueiro
{
    public class VerNoMapaModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public VerNoMapaModel(ApplicationDbContext context)
        {
            _context = context;
        }

        public PiscAtlas.Models.Models.Pesqueiro PesqueiroItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int id)
        {
            var pesqueiro = await _context.Pesqueiros.FindAsync(id);
            if (pesqueiro == null) return NotFound();

            PesqueiroItem = pesqueiro;
            return Page();
        }
    }
}