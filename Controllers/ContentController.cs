using Microsoft.AspNetCore.Mvc;

namespace Cinema.Controllers
{
    public class ContentController : Controller
    {
        public IActionResult Filmes() => View();
        public IActionResult Cinemas() => View();
        public IActionResult Bar() => View();
        public IActionResult Vantagens() => View();
        public IActionResult Novidades() => View();
        public IActionResult Produtos() => View();
        public IActionResult Corporativo() => View();
    }
}
