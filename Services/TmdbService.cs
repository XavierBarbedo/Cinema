using System;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Collections.Generic;
using Newtonsoft.Json.Linq;
using Cinema.Models;
using Cinema.Data;
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

        // Retorna uma lista de filmes: Primeiro da BD, só usa API se BD estiver vazia
        public async Task<List<Filme>> GetFilmesEmDestaque(int quantidade = 13)
        {
            var filmes = new List<Filme>();

            // 1. PRIMEIRO: VERIFICAR SE HÁ FILMES NA BASE DE DADOS
            var filmesDB = await _context.Filmes
                .OrderByDescending(f => f.DataLancamento)
                .ToListAsync();

            // 2. SE HOUVER FILMES NA BD, RETORNA ELES
            if (filmesDB.Any())
            {
                return filmesDB;
            }

            // 3. SE NÃO HOUVER FILMES NA BD, BUSCA DA API TMDB
            try
            {
                string url = $"https://api.themoviedb.org/3/movie/now_playing?api_key={_apiKey}&language=pt-PT";
                var json = JObject.Parse(await _http.GetStringAsync(url));
                var results = json["results"]?.Take(quantidade);

                if (results != null && results.Any())
                {
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
                                : "",
                            SempreNoCinema = true
                        };

                        filmes.Add(filme);
                        _context.Filmes.Add(filme);
                    }

                    await _context.SaveChangesAsync();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erro ao buscar filmes da API TMDB: {ex.Message}");
            }

            // 4. SE NÃO HOUVER NENHUM FILME (nem BD nem API), RETORNA UM FALLBACK
            if (!filmes.Any())
            {
                filmes.Add(new Filme
                {
                    Titulo = "Nenhum filme disponível",
                    Genero = "-",
                    Duracao = 0,
                    Sinopse = "Adicione filmes através do painel de administração.",
                    Capa = "/images/fallback.jpg",
                    Background = "/images/fallback.jpg",
                    DataLancamento = DateTime.MinValue,
                    Elenco = "-",
                    Realizador = "-",
                    TrailerYoutubeId = "",
                    SempreNoCinema = false
                });
            }

            return filmes;
        }

        // Método alternativo: Retorna TODOS os filmes (API + BD) sem limite
        public async Task<List<Filme>> GetTodosOsFilmes()
        {
            var filmes = new List<Filme>();

            // 1. Buscar filmes da API
            var filmesApi = await GetFilmesEmDestaque(20); // Busca mais filmes da API
            filmes.AddRange(filmesApi.Where(f => f.SempreNoCinema == true));

            // 2. Buscar TODOS os filmes da base de dados
            var filmesDB = await _context.Filmes.ToListAsync();

            // 3. Remover duplicados (caso um filme da BD já esteja na API)
            var titulosApi = filmes.Select(f => f.Titulo).ToHashSet();
            var filmesDBUnicos = filmesDB.Where(f => !titulosApi.Contains(f.Titulo));

            filmes.AddRange(filmesDBUnicos);

            return filmes.OrderByDescending(f => f.DataLancamento).ToList();
        }
    }
}