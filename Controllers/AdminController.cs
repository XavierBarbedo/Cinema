
using Cinema.Attributes; // Caso uses AdminOnly
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;
using System.Collections.Generic;
using System.Threading.Tasks;



namespace Cinema.Controllers
{


    [AdminOnly]
    public class AdminController : Controller
    {
        private readonly CinemaDbContext _context;

        public AdminController(CinemaDbContext context)
        {
            _context = context;
        }


        public IActionResult Home()
        {
            return View();
        }

        // -------------------------
        // FUTURAS EXTENSÕES DO PAINEL DE ADMIN
        // -------------------------
        // Estes métodos não precisam ser implementados agora,
        // mas já deixo preparados para tua estrutura:


        public IActionResult Sessoes()
        {
            return View();
        }

        public IActionResult Reservas()
        {
            return View();
        }
        // GET: /Admin/Filmes
        public async Task<IActionResult> Filmes()
        {
            // Busca todos os filmes da base de dados
            List<Filme> filmes = await _context.Filmes.ToListAsync();

            return View(filmes); // Passa a lista para a view
        }
        // POST: /Admin/Filmes/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var filme = _context.Filmes.Find(id);
            if (filme != null)
            {
                _context.Filmes.Remove(filme);
                _context.SaveChanges();
                TempData["SuccessMessage"] = $"Filme '{filme.Titulo}' foi apagado com sucesso!";
            }
            return RedirectToAction("Filmes");
        }

        // GET: Criar novo filme
        public IActionResult FilmeForm()
        {
            return View(new Filme());
        }

        // GET: Editar filme existente
        public IActionResult FilmeForm(int id)
        {
            var filme = _context.Filmes.Find(id);
            if (filme == null)
                return NotFound();
            return View(filme);
        }

        // POST: Guardar (criar ou editar)
        [HttpPost]
        public IActionResult SaveFilme(Filme filme)
        {
            if (ModelState.IsValid)
            {
                if (filme.Id == 0)
                    _context.Filmes.Add(filme);
                else
                    _context.Filmes.Update(filme);

                _context.SaveChanges();
                TempData["SuccessMessage"] = "Filme guardado com sucesso!";
                return RedirectToAction("FilmesDashboard");
            }
            return View("FilmeForm", filme);
        }
    }
}
