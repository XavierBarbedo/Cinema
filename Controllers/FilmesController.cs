using Microsoft.AspNetCore.Mvc;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json.Linq;
using System.Linq;
using System.Collections.Generic;
using Cinema.Models;

namespace Cinema.Controllers
{
    public class FilmesController : Controller
    {
        private readonly HttpClient _http;

    public FilmesController()
        {
            _http = new HttpClient();
        }

        // GET: /Filmes/Buscar?q=...
        [HttpGet]
        public async Task<IActionResult> Buscar(string q)
        {
            if (string.IsNullOrWhiteSpace(q))
                return Json(new List<Filme>());

            // Exemplo de chamada à API externa (TMDB ou outra)
            // Substitui com a tua API real ou base de dados
            string apiKey = "71f3506dc79629c7fee2aef814947505";
            string url = $"https://api.themoviedb.org/3/search/movie?api_key={apiKey}&query={q}";

            var response = await _http.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return StatusCode(500, "Erro ao consultar a API de filmes.");

            var json = await response.Content.ReadAsStringAsync();
            var data = JObject.Parse(json);
            var results = data["results"]?.Take(1).Select(f => new Filme
            {
                Id = (int)f["id"],
                Titulo = (string)f["title"],
                Genero = f["genre_ids"] != null ? string.Join(", ", f["genre_ids"]) : "Desconhecido",
                Duracao = f["runtime"] != null ? (int)f["runtime"] : 0,
                Elenco = "Elenco não disponível", // podes consultar outra API endpoint para elenco
                Realizador = "Realizador não disponível", // idem
                Capa = f["poster_path"] != null ? $"https://image.tmdb.org/t/p/w500{f["poster_path"]}" : "",
                TrailerYoutubeId = "" // opcional: podes chamar outro endpoint para trailer
            }).ToList();

            return Json(results);
        }
    }

}
