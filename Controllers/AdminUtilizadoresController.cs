using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Attributes;
using Cinema.Helpers;

namespace Cinema.Controllers
{
    [AdminOnly]
    public class AdminUtilizadoresController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminUtilizadoresController(CinemaDbContext context)
        {
            _context = context;
        }

        // GET: AdminUtilizadores
        public async Task<IActionResult> Index(string role)
        {
            var query = _context.Utilizadores.AsQueryable();

            if (!string.IsNullOrEmpty(role))
            {
                query = query.Where(u => u.Role == role);
            }

            var utilizadores = await query
                .OrderBy(u => u.Nome)
                .ToListAsync();

            ViewBag.RoleSelecionado = role;

            return View(utilizadores);
        }

        // GET: AdminUtilizadores/Detalhes/5
        public async Task<IActionResult> Detalhes(int? id)
        {
            if (id == null) return NotFound();

            var utilizadorAtualId = HttpContext.Session.GetUserId();

            var utilizador = await _context.Utilizadores
                .Include(u => u.Reservas)
                    .ThenInclude(r => r.Sessao)
                        .ThenInclude(s => s.Filme)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (utilizador == null) return NotFound();

            // Não permitir ver detalhes do próprio utilizador desta forma
            if (utilizador.Id == utilizadorAtualId)
            {
                return RedirectToAction("Perfil", "Utilizador");
            }

            return View(utilizador);
        }

        // GET: AdminUtilizadores/AlterarRole/5
        public async Task<IActionResult> AlterarRole(int? id)
        {
            if (id == null) return NotFound();

            var utilizadorAtualId = HttpContext.Session.GetUserId();

            // Não permitir alterar o próprio role
            if (id == utilizadorAtualId)
            {
                TempData["Erro"] = "Não pode alterar o seu próprio role.";
                return RedirectToAction(nameof(Index));
            }

            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return NotFound();

            return View(utilizador);
        }

        // POST: AdminUtilizadores/AlterarRole/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AlterarRole(int id, string novoRole)
        {
            var utilizadorAtualId = HttpContext.Session.GetUserId();

            if (id == utilizadorAtualId)
            {
                TempData["Erro"] = "Não pode alterar o seu próprio role.";
                return RedirectToAction(nameof(Index));
            }

            var utilizador = await _context.Utilizadores.FindAsync(id);

            if (utilizador == null) return NotFound();

            if (novoRole != "Cliente" && novoRole != "Administrador")
            {
                TempData["Erro"] = "Role inválido.";
                return RedirectToAction(nameof(AlterarRole), new { id });
            }

            utilizador.Role = novoRole;
            _context.Update(utilizador);
            await _context.SaveChangesAsync();

            TempData["Sucesso"] = $"Role alterado para {novoRole} com sucesso!";
            return RedirectToAction(nameof(Index));
        }
    }
}