using Cinema.Attributes;
using Cinema.Data;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

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


        // GET: /Admin/Sessoes
        public async Task<IActionResult> Sessoes()
        {
            // Usar .Include(f => f.Sessoes) para garantir que as sessões são carregadas
            // juntamente com os filmes na mesma consulta à base de dados.
            var filmes = await _context.Filmes
                                       .Include(f => f.Sessoes)
                                       .ToListAsync();

            return View(filmes);
        }

        // GET: /Admin/Reservas
        public async Task<IActionResult> Reservas()
        {
            var reservas = await _context.Reservas
                                         .Include(r => r.Utilizador)
                                         .Include(r => r.Sessao)
                                            .ThenInclude(s => s.Filme)
                                         .OrderByDescending(r => r.DataReserva)
                                         .ToListAsync();

            return View(reservas);
        }

        // POST: /Admin/DeleteReserva/5
        [HttpPost]
        public IActionResult DeleteReserva(int id)
        {
            var reserva = _context.Reservas.Find(id);
            if (reserva != null)
            {
                // Devolver lugares à sessão
                var sessao = _context.Sessoes.Find(reserva.SessaoId);
                if (sessao != null) 
                { 
                    sessao.LugaresDisponiveis += reserva.NumeroBilhetes; 
                    _context.Sessoes.Update(sessao);
                }

                _context.Reservas.Remove(reserva);
                _context.SaveChanges();
                TempData["SuccessMessage"] = "Reserva cancelada com sucesso e lugares repostos!";
            }
            return RedirectToAction("Reservas");
        }

        // GET: /Admin/Filmes
        public async Task<IActionResult> Filmes()
        {
            var filmes = await _context.Filmes.ToListAsync();
            return View(filmes);
        }

        // POST: /Admin/Filmes/Delete/5
        [HttpPost]
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
        public IActionResult Create()
        {
            return View("FilmeForm", new Filme());
        }

        // GET: Editar filme existente
        public IActionResult Edit(int id)
        {
            var filme = _context.Filmes.Find(id);
            if (filme == null)
                return NotFound();

            return View("FilmeForm", filme);
        }

        // POST: Salvar filme (criar ou editar)
        [HttpPost]
        public IActionResult SaveFilme(Filme filme)
        {
            if (!ModelState.IsValid)
            {
                // Se houver erros de validação, retorna à view com os erros
                return View("FilmeForm", filme);
            }

            try
            {
                if (filme.Id > 0)
                {
                    // EDITAR - filme existente
                    var filmeExistente = _context.Filmes.Find(filme.Id);

                    if (filmeExistente == null)
                    {
                        TempData["ErrorMessage"] = "Filme não encontrado!";
                        return RedirectToAction("Filmes");
                    }

                    // Atualizar todas as propriedades
                    filmeExistente.Titulo = filme.Titulo;
                    filmeExistente.Genero = filme.Genero;
                    filmeExistente.Duracao = filme.Duracao;
                    filmeExistente.Sinopse = filme.Sinopse;
                    filmeExistente.Realizador = filme.Realizador;
                    filmeExistente.DataLancamento = filme.DataLancamento;
                    filmeExistente.Elenco = filme.Elenco;
                    filmeExistente.Capa = filme.Capa;
                    filmeExistente.Background = filme.Background;
                    filmeExistente.TrailerYoutubeId = filme.TrailerYoutubeId;
                    filmeExistente.SempreNoCinema = false;

                    _context.Filmes.Update(filmeExistente);
                    TempData["SuccessMessage"] = $"Filme '{filme.Titulo}' atualizado com sucesso!";
                }
                else
                {
                    // CRIAR - novo filme
                    _context.Filmes.Add(filme);
                    TempData["SuccessMessage"] = $"Filme '{filme.Titulo}' criado com sucesso!";
                }

                _context.SaveChanges();
                return RedirectToAction("Filmes");
            }
            catch (Exception ex)
            {
                // Em caso de erro, mostra mensagem e retorna à view
                TempData["ErrorMessage"] = $"Erro ao salvar filme: {ex.Message}";
                return View("FilmeForm", filme);
            }
        }

        // GET: Formulário de Sessão (novo ou editar)
        public async Task<IActionResult> SessaoForm(int? id, int? filmeId)
        {
            ViewBag.Filmes = await _context.Filmes.ToListAsync();

            Sessao sessao;

            if (id.HasValue)
            {
                sessao = await _context.Sessoes.FindAsync(id.Value);
                if (sessao == null) return NotFound();
            }
            else
            {
                sessao = new Sessao
                {
                    FilmeId = filmeId ?? 0,
                    DataHora = DateTime.Now.AddDays(7),
                    Preco = 7,
                    CapacidadeTotal = 100,
                    LugaresDisponiveis = 100
                };
            }

            return View(sessao);
        }





        // GET: Criar sessão
        public IActionResult CriarSessao(int filmeId)
        {
            // Guardar o FilmeId na Session
            HttpContext.Session.SetInt32("FilmeIdSelecionado", filmeId);

            // Redirecionar para o formulário
            return RedirectToAction("SessaoForm", new { id = (int?)null, filmeId = filmeId });
        }


        public IActionResult EditarSessao(int id)
        {
            return RedirectToAction("SessaoForm", new { id = id });
        }

        // POST: Salvar Sessao
        [HttpPost]
        public IActionResult SaveSessao(Sessao sessao)
        {
            // Trazer lista de filmes caso haja erro de validação
            ViewBag.Filmes = _context.Filmes.ToList();

            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Existem erros no formulário. Verifique os campos.";
                return View("SessaoForm", sessao);
            }

            // Validar se o FilmeId existe
            var filme = _context.Filmes.Find(sessao.FilmeId);
            if (filme == null)
            {
                TempData["ErrorMessage"] = "Filme não encontrado!";
                return View("SessaoForm", sessao);
            }

            if (sessao.Id > 0)
            {
                // Editar sessão existente
                var existente = _context.Sessoes.Find(sessao.Id);
                existente.Sala = sessao.Sala;
                existente.DataHora = sessao.DataHora;
                existente.Preco = sessao.Preco;
                existente.CapacidadeTotal = sessao.CapacidadeTotal;
                existente.FilmeId = sessao.FilmeId;

                if (existente.CapacidadeTotal != sessao.CapacidadeTotal)
                    existente.LugaresDisponiveis = sessao.CapacidadeTotal;

                _context.Sessoes.Update(existente);
                TempData["SuccessMessage"] = "Sessão atualizada com sucesso!";
            }
            else
            {
                // Criar nova sessão
                sessao.LugaresDisponiveis = sessao.CapacidadeTotal;
                _context.Sessoes.Add(sessao);
                TempData["SuccessMessage"] = "Sessão criada com sucesso!";
            }

            _context.SaveChanges();
            return RedirectToAction("Sessoes");
        }



        // POST: Apagar Sessão
        [HttpPost]
        public IActionResult DeleteSessao(int id)
        {
            var sessao = _context.Sessoes.Find(id);
            if (sessao != null)
            {
                _context.Sessoes.Remove(sessao);
                _context.SaveChanges();
            }

            return RedirectToAction("Sessoes");
        }

    }
}