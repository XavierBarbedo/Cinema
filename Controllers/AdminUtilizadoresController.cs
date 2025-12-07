using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Attributes;
using Cinema.Helpers;

namespace Cinema.Controllers
{
    // Apenas administradores podem aceder a este controller
    [AdminOnly]
    public class AdminUtilizadoresController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminUtilizadoresController(CinemaDbContext context)
        {
            _context = context;
        }

        // Painel principal dos utilizadores
        public async Task<IActionResult> Index(string role)
        {
            var query = _context.Utilizadores.AsQueryable();

            if (!string.IsNullOrEmpty(role))
                query = query.Where(u => u.Role == role);

            var utilizadores = await query
                .OrderBy(u => u.Nome)
                .ToListAsync();

            ViewBag.RoleSelecionado = role;

            return View(utilizadores);
        }

        // Ver detalhes de um utilizador
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

            // Impedir ver os detalhes do próprio admin
            if (utilizador.Id == utilizadorAtualId)
                return RedirectToAction("Perfil", "Utilizador");

            return View(utilizador);
        }

        // Mostrar página para alterar role
        public async Task<IActionResult> AlterarRole(int? id)
        {
            if (id == null) return NotFound();

            var utilizadorAtualId = HttpContext.Session.GetUserId();

            if (id == utilizadorAtualId)
            {
                TempData["Erro"] = "Não pode alterar o seu próprio role.";
                return RedirectToAction(nameof(Index));
            }

            var utilizador = await _context.Utilizadores.FindAsync(id);
            if (utilizador == null) return NotFound();

            return View(utilizador);
        }

        // Aplicar mudança de role
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

            TempData["Sucesso"] = $"O role do utilizador foi alterado para {novoRole}.";
            return RedirectToAction(nameof(Index));
        }


    }
}
