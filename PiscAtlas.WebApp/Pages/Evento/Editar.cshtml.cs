using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using System.ComponentModel.DataAnnotations;

namespace PiscAtlas.WebApp.Pages.Evento
{
    [Authorize(Roles = "Admin")]
    public class EditarModel : PageModel
    {
        private readonly ApplicationDbContext _context;

        public EditarModel(ApplicationDbContext context)
        {
            _context = context;
        }

        [BindProperty]
        public int EventoId { get; set; }

        [BindProperty]
        public EventoInputModel Input { get; set; } = new();

        public SelectList Especies { get; set; } = default!;

        public async Task<IActionResult> OnGetAsync(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return NotFound();

            EventoId = evento.EventoId;
            Input = new EventoInputModel
            {
                Nome = evento.Nome,
                Descricao = evento.Descricao,
                DataInicio = evento.DataInicio,
                DataFim = evento.DataFim,
                EspecieAlvoId = evento.EspecieAlvoId,
                PesoMinimo = evento.PesoMinimo,
                TamanhoMinimo = evento.TamanhoMinimo,
                PrecoInscricao = evento.PrecoInscricao
            };

            await PopularSelectLists();
            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (Input.DataFim < Input.DataInicio)
            {
                ModelState.AddModelError("Input.DataFim", "A data de fim não pode ser anterior à data de início.");
            }

            if (!ModelState.IsValid)
            {
                await PopularSelectLists();
                return Page();
            }

            var evento = await _context.Eventos.FindAsync(EventoId);
            if (evento == null) return NotFound();

            evento.Nome = Input.Nome;
            evento.Descricao = Input.Descricao ?? "";
            evento.DataInicio = Input.DataInicio;
            evento.DataFim = Input.DataFim;
            evento.EspecieAlvoId = Input.EspecieAlvoId;
            evento.PesoMinimo = Input.PesoMinimo;
            evento.TamanhoMinimo = Input.TamanhoMinimo;
            evento.PrecoInscricao = Input.PrecoInscricao ?? 0;

            _context.Update(evento);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Evento atualizado com sucesso!";
            return RedirectToPage("./Detalhes", new { id = EventoId });
        }

        private async Task PopularSelectLists()
        {
            Especies = new SelectList(await _context.Especies.OrderBy(e => e.Nome).ToListAsync(), "EspecieId", "Nome");
        }

        public class EventoInputModel
        {
            [Required(ErrorMessage = "O nome é obrigatório.")]
            public string Nome { get; set; } = string.Empty;

            public string? Descricao { get; set; }

            [Required(ErrorMessage = "A data de início é obrigatória.")]
            public DateTime DataInicio { get; set; }

            [Required(ErrorMessage = "A data de fim é obrigatória.")]
            public DateTime DataFim { get; set; }

            [Required(ErrorMessage = "Selecione a espécie-alvo.")]
            public int EspecieAlvoId { get; set; }

            public double? PesoMinimo { get; set; }
            public double? TamanhoMinimo { get; set; }
            public decimal? PrecoInscricao { get; set; }
        }
    }
}