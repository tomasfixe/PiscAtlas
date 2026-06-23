using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;

namespace PiscAtlas.WebApp.Controllers
{
    public class EventoController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<Utilizador> _userManager;

        public EventoController(ApplicationDbContext context, UserManager<Utilizador> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Evento/Index — lista todos os eventos (futuros primeiro)
        public async Task<IActionResult> Index()
        {
            var eventos = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes)
                .OrderBy(e => e.DataInicio)
                .ToListAsync();

            return View(eventos);
        }

        // GET: Evento/Detalhes/5
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            // Verifica se o utilizador atual já está inscrito
            if (User.Identity?.IsAuthenticated == true)
            {
                var user = await _userManager.GetUserAsync(User);
                ViewBag.JaInscrito = evento.Inscricoes
                    .Any(i => i.PescadorEmail == user!.Email);
            }

            return View(evento);
        }

        // POST: Evento/Inscrever/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize]
        public async Task<IActionResult> Inscrever(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();

            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Challenge();

            // Impede inscrição dupla
            bool jaInscrito = evento.Inscricoes.Any(i => i.PescadorEmail == user.Email);
            if (jaInscrito)
            {
                TempData["Erro"] = "Já está inscrito neste evento.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            // Impede inscrição em evento passado
            if (evento.DataInicio < DateTime.Now)
            {
                TempData["Erro"] = "Não é possível inscrever-se num evento já terminado.";
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            var inscricao = new Inscricao
            {
                EventoId      = evento.EventoId,
                PescadorEmail = user.Email!,
                PescadorNome  = user.NomeCompleto,
                EstadoPagamento = EstadoPagamento.Pendente,
                ValorPago     = evento.PrecoInscricao
            };

            _context.Inscricoes.Add(inscricao);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Inscrição realizada com sucesso! Aguarda confirmação de pagamento.";
            return RedirectToAction(nameof(Detalhes), new { id });
        }

        // --- Área Admin: Criar/Editar/Eliminar Eventos e Torneios ---

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar()
        {
            await PopularEspecies();
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Criar(EventoViewModel model)
        {
            if (model.DataFim < model.DataInicio)
            {
                ModelState.AddModelError(nameof(model.DataFim), "A data de fim não pode ser anterior à data de início.");
            }

            if (ModelState.IsValid)
            {
                var evento = new Evento
                {
                    Nome            = model.Nome,
                    Descricao       = model.Descricao,
                    DataInicio      = model.DataInicio,
                    DataFim         = model.DataFim,
                    EspecieAlvoId   = model.EspecieAlvoId,
                    PesoMinimo      = model.PesoMinimo,
                    TamanhoMinimo   = model.TamanhoMinimo,
                    PrecoInscricao  = model.PrecoInscricao
                };

                _context.Eventos.Add(evento);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Evento criado com sucesso!";
                return RedirectToAction(nameof(Index));
            }

            await PopularEspecies();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return NotFound();

            var model = new EventoViewModel
            {
                Nome           = evento.Nome,
                Descricao      = evento.Descricao,
                DataInicio     = evento.DataInicio,
                DataFim        = evento.DataFim,
                EspecieAlvoId  = evento.EspecieAlvoId,
                PesoMinimo     = evento.PesoMinimo,
                TamanhoMinimo  = evento.TamanhoMinimo,
                PrecoInscricao = evento.PrecoInscricao
            };

            ViewBag.EventoId = evento.EventoId;
            await PopularEspecies();
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Editar(int id, EventoViewModel model)
        {
            var evento = await _context.Eventos.FindAsync(id);
            if (evento == null) return NotFound();

            if (model.DataFim < model.DataInicio)
            {
                ModelState.AddModelError(nameof(model.DataFim), "A data de fim não pode ser anterior à data de início.");
            }

            if (ModelState.IsValid)
            {
                evento.Nome            = model.Nome;
                evento.Descricao       = model.Descricao;
                evento.DataInicio      = model.DataInicio;
                evento.DataFim         = model.DataFim;
                evento.EspecieAlvoId   = model.EspecieAlvoId;
                evento.PesoMinimo      = model.PesoMinimo;
                evento.TamanhoMinimo   = model.TamanhoMinimo;
                evento.PrecoInscricao  = model.PrecoInscricao;

                _context.Update(evento);
                await _context.SaveChangesAsync();

                TempData["Sucesso"] = "Evento atualizado com sucesso!";
                return RedirectToAction(nameof(Detalhes), new { id });
            }

            ViewBag.EventoId = id;
            await PopularEspecies();
            return View(model);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Eliminar(int? id)
        {
            if (id == null) return NotFound();

            var evento = await _context.Eventos
                .Include(e => e.EspecieAlvo)
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento == null) return NotFound();
            return View(evento);
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> EliminarConfirmado(int id)
        {
            var evento = await _context.Eventos
                .Include(e => e.Inscricoes)
                .FirstOrDefaultAsync(e => e.EventoId == id);

            if (evento != null)
            {
                _context.Inscricoes.RemoveRange(evento.Inscricoes);
                _context.Eventos.Remove(evento);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Evento eliminado.";
            }
            return RedirectToAction(nameof(Index));
        }

        private async Task PopularEspecies()
        {
            ViewBag.Especies = new SelectList(
                await _context.Especies.OrderBy(e => e.Nome).ToListAsync(),
                "EspecieId", "Nome");
        }
    }

    // ViewModel para criar/editar evento
    public class EventoViewModel
    {
        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "O nome é obrigatório.")]
        public string Nome { get; set; } = string.Empty;

        public string? Descricao { get; set; }

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "A data de início é obrigatória.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Data de Início")]
        public DateTime DataInicio { get; set; } = DateTime.Now;

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "A data de fim é obrigatória.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Data de Fim")]
        public DateTime DataFim { get; set; } = DateTime.Now.AddDays(1);

        [System.ComponentModel.DataAnnotations.Required(ErrorMessage = "Selecione a espécie-alvo.")]
        [System.ComponentModel.DataAnnotations.Display(Name = "Espécie-Alvo")]
        public int EspecieAlvoId { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Peso Mínimo (kg)")]
        public double? PesoMinimo { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Tamanho Mínimo (cm)")]
        public double? TamanhoMinimo { get; set; }

        [System.ComponentModel.DataAnnotations.Display(Name = "Preço de Inscrição (€)")]
        public decimal PrecoInscricao { get; set; }
    }
}
