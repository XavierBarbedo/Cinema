using System.Diagnostics;
using Cinema.Models;
using Microsoft.AspNetCore.Mvc;
using Cinema.Services;
using System.Threading.Tasks;

namespace Cinema.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly TmdbService _tmdb;

    public HomeController(ILogger<HomeController> logger, TmdbService tmdb)
        {
            _logger = logger;
            _tmdb = tmdb;
        }

        public async Task<IActionResult> Index()
        {
            // Supondo que você crie um método que retorne vários filmes
            var filmes = await _tmdb.GetFilmesEmDestaque(); // List<Filme>
            return View(filmes);
        }


        private Filme CriarFilmeFallback()
        {
            return new Filme
            {
                Id = 0,
                Titulo = "Filme não disponível",
                Genero = "-",
                Duracao = 0,
                Sinopse = "-",
                Capa = "/images/fallback.jpg",
                Background = "/images/fallback.jpg",
                DataLancamento = System.DateTime.MinValue,
                Elenco = "-",
                Realizador = "-",
                TrailerYoutubeId = ""
            };
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }

}
