using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Attributes;

namespace Cinema.Controllers
{
    [AdminOnly]
    public class AdminReservasController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminReservasController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: AdminReservas
        public async Task<IActionResult> Index(string estado, int? utilizadorId)
        {
            var query = _context.Reservas
                .Include(r => r.Utilizador)
                .Include(r => r.Sessao)
                    .ThenInclude(s => s.Filme)
                .AsQueryable();

            if (!string.IsNullOrEmpty(estado))
            {
                query = query.Where(r => r.Estado == estado);
            }

            if (utilizadorId.HasValue)
            {
                query = query.Where(r => r.UtilizadorId == utilizadorId);
            }

            var reservas = await query
                .OrderByDescending(r => r.DataReserva)
                .ToListAsync();

            ViewBag.Utilizadores = await _context.Utilizadores
                .Where(u => u.Role == "Cliente")
                .OrderBy(u => u.Nome)
                .ToListAsync();

            ViewBag.EstadoSelecionado = estado;
            ViewBag.UtilizadorIdSelecionado = utilizadorId;

            return View(reservas);
        }

        // GET: AdminReservas/Detalhes/5
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var reserva = await _context.Reservas
                .Include(r => r.Utilizador)
                .Include(r => r.Sessao)
                    .ThenInclude(s => s.Filme)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (reserva == null) return NotFound();

            return View(reserva);
        }

        // POST: AdminReservas/Cancelar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Cancelar(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Sessao)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva == null) return NotFound();

            if (reserva.Estado == "Cancelada")
            {
                TempData["Erro"] = "Esta reserva já está cancelada.";
                return RedirectToAction(nameof(Index));
            }

            // Devolver lugares à sessão
            reserva.Sessao.LugaresDisponiveis += reserva.NumeroBilhetes;
            reserva.Estado = "Cancelada";

            _context.Update(reserva);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = "Reserva cancelada com sucesso.";
            return RedirectToAction(nameof(Index));
        }

        // POST: AdminReservas/Eliminar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Eliminar(int id)
        {
            var reserva = await _context.Reservas
                .Include(r => r.Sessao)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (reserva != null)
            {
                // Se a reserva está confirmada, devolver lugares
                if (reserva.Estado == "Confirmada")
                {
                    reserva.Sessao.LugaresDisponiveis += reserva.NumeroBilhetes;
                    _context.Update(reserva.Sessao);
                }

                _context.Reservas.Remove(reserva);
                await _context.SaveChangesAsync();
                TempData["Sucesso"] = "Reserva eliminada com sucesso.";
            }

            return RedirectToAction(nameof(Index));
        }
    }
}
