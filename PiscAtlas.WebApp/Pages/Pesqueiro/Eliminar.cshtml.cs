using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;

namespace PiscAtlas.WebApp.Pages.Pesqueiro
{
    [Authorize(Roles = "Admin")]
    public class EliminarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EliminarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int PesqueiroId { get; set; }

        public PiscAtlas.Models.Models.Pesqueiro PesqueiroItem { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();
            var pesqueiro = await _context.Pesqueiros.Include(p => p.Capturas).FirstOrDefaultAsync(p => p.PesqueiroId == id);
            if (pesqueiro == null) return NotFound();
            PesqueiroItem = pesqueiro;
            PesqueiroId = pesqueiro.PesqueiroId;
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            var pesqueiro = await _context.Pesqueiros.FindAsync(PesqueiroId);
            if (pesqueiro != null)
            {
                _context.Pesqueiros.Remove(pesqueiro);
                await _context.SaveChangesAsync();
            }
            return RedirectToPage("./Index");
        }
    }
}