using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Cinema.Models;
using Cinema.Data; // teu DbContext
using Microsoft.EntityFrameworkCore;

namespace Cinema.Services
{
    public class TmdbService
    {
        private readonly string _apiKey = "71f3506dc79629c7fee2aef814947505";
        private readonly HttpClient _http;
        private readonly CinemaDbContext _context;

        public TmdbService(HttpClient http, CinemaDbContext context)
        {
            _http = http;
            _context = context;
        }

        // Retorna uma lista de filmes em destaque
        public async Task<List<Filme>> GetFilmesEmDestaque(int quantidade = 13)
        {
            string url = $"https://api.themoviedb.org/3/movie/now_playing?api_key={_apiKey}&language=pt-PT";
            var json = JObject.Parse(await _http.GetStringAsync(url));

            var results = json["results"]?.Take(quantidade);
            var filmes = new List<Filme>();

            if (results == null || !results.Any())
            {
                filmes.Add(new Filme
                {
                    Titulo = "Filme não disponível",
                    Genero = "-",
                    Duracao = 0,
                    Sinopse = "-",
                    Capa = "/images/fallback.jpg",
                    Background = "/images/fallback.jpg",
                    DataLancamento = DateTime.MinValue,
                    Elenco = "-",
                    Realizador = "-",
                    TrailerYoutubeId = ""
                });
                return filmes;
            }

            foreach (var filmeApi in results)
            {
                int movieId = (int)filmeApi["id"];
                string detalhesUrl = $"https://api.themoviedb.org/3/movie/{movieId}?api_key={_apiKey}&language=pt-PT&append_to_response=videos,credits";
                var dados = JObject.Parse(await _http.GetStringAsync(detalhesUrl));

                var filme = new Filme
                {
                    Titulo = (string)dados["title"] ?? "Título não disponível",
                    Genero = dados["genres"]?.FirstOrDefault()?["name"]?.ToString() ?? "-",
                    Duracao = dados["runtime"] != null ? (int)dados["runtime"] : 0,
                    Sinopse = (string)dados["overview"] ?? "-",
                    Capa = dados["poster_path"] != null ? "https://image.tmdb.org/t/p/w500" + (string)dados["poster_path"] : "/images/fallback.jpg",
                    Background = dados["backdrop_path"] != null ? "https://image.tmdb.org/t/p/original" + (string)dados["backdrop_path"] : "/images/fallback.jpg",
                    DataLancamento = DateTime.TryParse((string)dados["release_date"], out var dt) ? dt : DateTime.MinValue,
                    Elenco = dados["credits"]?["cast"] != null
                        ? string.Join(", ", dados["credits"]["cast"].Take(5).Select(a => (string)a["name"]))
                        : "-",
                    Realizador = dados["credits"]?["crew"] != null
                        ? (string)dados["credits"]["crew"].FirstOrDefault(c => (string)c["job"] == "Director")?["name"] ?? "-"
                        : "-",
                    TrailerYoutubeId = dados["videos"]?["results"] != null
                        ? (string)dados["videos"]["results"]
                            .FirstOrDefault(v => (string)v["site"] == "YouTube" && (string)v["type"] == "Trailer")?["key"]
                        : ""
                };

                filmes.Add(filme);

                //  Salva na base de dados se não existir
                if (!await _context.Filmes.AnyAsync(f => f.Titulo == filme.Titulo && f.DataLancamento == filme.DataLancamento))
                {
                    _context.Filmes.Add(filme);
                }
            }

            await _context.SaveChangesAsync();
            return filmes;
        }
    }
}
