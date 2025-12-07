using Cinema.Attributes;
using Microsoft.AspNetCore.Mvc;

[AdminOnly]
public class AdminFilmesController : Controller
{
    public IActionResult Filmes()
    {
        RedirectToAction("Filmes", "Admin");

        return View();
    }
}
