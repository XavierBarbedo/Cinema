using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Cinema.Controllers
{
    public class BilhetesController : Controller
    {
        private readonly CinemaDbContext _context;

        public BilhetesController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: /Bilhetes
        // Lista todos os filmes em cartaz que têm sessões futuras
        public async Task<IActionResult> Index()
        {
            var filmesComSessoes = await _context.Filmes
                .Include(f => f.Sessoes)
                .Where(f => f.Sessoes.Any(s => s.DataHora > DateTime.Now))
                .ToListAsync();

            return View(filmesComSessoes);
        }

        // GET: /Bilhetes/Detalhes/{filmeId}
        public async Task<IActionResult> Detalhes(int id)
        {
            var filme = await _context.Filmes
                .Include(f => f.Sessoes)
                .FirstOrDefaultAsync(f => f.Id == id);

            if (filme == null) return NotFound();

            return View(filme);
        }

        // GET: /Bilhetes/Comprar/{sessaoId}
        public async Task<IActionResult> Comprar(int id)
        {
            // Verificar Login
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Utilizador", new { returnUrl = $"/Bilhetes/Comprar/{id}" });
            }

            var sessao = await _context.Sessoes
                .Include(s => s.Filme)
                .Include(s => s.Reservas)
                .FirstOrDefaultAsync(s => s.Id == id);

            if (sessao == null) return NotFound();

            // Obter lugares já ocupados
            var lugaresOcupados = sessao.Reservas
                .Where(r => !string.IsNullOrEmpty(r.LugaresMarcados))
                .SelectMany(r => r.LugaresMarcados.Split(','))
                .ToList();

            ViewBag.LugaresOcupados = lugaresOcupados;

            return View(sessao);
        }

        // POST: /Bilhetes/Confirmar
        [HttpPost]
        public async Task<IActionResult> Confirmar(int sessaoId, string lugaresSelecionados)
        {
            // Verificar Login
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Utilizador");
            }
            if (string.IsNullOrEmpty(lugaresSelecionados))
            {
                TempData["ErrorMessage"] = "Selecione pelo menos um lugar.";
                return RedirectToAction("Comprar", new { id = sessaoId });
            }

            var sessao = await _context.Sessoes.FindAsync(sessaoId);
            if (sessao == null) return NotFound();

            var lugaresArray = lugaresSelecionados.Split(',', StringSplitOptions.RemoveEmptyEntries);
            int qtdBilhetes = lugaresArray.Length;

            // Utilizador já autenticado
            var utilizador = await _context.Utilizadores.FindAsync(userId);
            if (utilizador == null) return RedirectToAction("Login", "Utilizador");

            var reserva = new Reserva
            {
                SessaoId = sessaoId,
                UtilizadorId = utilizador.Id,
                NumeroBilhetes = qtdBilhetes,
                LugaresMarcados = lugaresSelecionados,
                ValorTotal = qtdBilhetes * sessao.Preco,
                DataReserva = DateTime.Now,
                Estado = "Confirmada"
            };

            // Atualizar lugares disponíveis
            sessao.LugaresDisponiveis -= qtdBilhetes;

            _context.Reservas.Add(reserva);
            _context.Sessoes.Update(sessao);
            await _context.SaveChangesAsync();

            // Auto-login (simples) para que o utilizador veja a reserva imediatamente
            HttpContext.Session.SetInt32("UserId", utilizador.Id);
            HttpContext.Session.SetString("UserName", utilizador.Nome);
            HttpContext.Session.SetString("UserRole", utilizador.Role);

            return View("Sucesso", reserva);
        }

        // GET: /Bilhetes/MinhasReservas
        public async Task<IActionResult> MinhasReservas()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId == null)
            {
                return RedirectToAction("Login", "Utilizador");
            }

            var reservas = await _context.Reservas
                .Include(r => r.Sessao)
                .ThenInclude(s => s.Filme)
                .Where(r => r.UtilizadorId == userId)
                .OrderByDescending(r => r.DataReserva)
                .ToListAsync();

            return View(reservas);
        }
    }
}
