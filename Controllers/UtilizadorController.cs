using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Models;
using Cinema.Models.ViewModels;

public class UtilizadorController : Controller
{
    private readonly CinemaDbContext _context;

    public UtilizadorController(CinemaDbContext context)
    {
        _context = context;
    }

    // GET: Login
    [HttpGet]
    public IActionResult Login(string email = "")
    {
        var model = new LoginViewModel { Email = email };
        return View(model);
    }

    // POST: Login
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _context.Utilizadores
            .FirstOrDefaultAsync(u => u.Email == model.Email && u.Password == model.Password);

        if (user == null)
        {
            ModelState.AddModelError("", "Credenciais inválidas");
            return View(model);
        }

        // Guardar na sessão
        HttpContext.Session.SetInt32("UserId", user.Id);
        HttpContext.Session.SetString("UserRole", user.Role);

        return RedirectToAction("Index", "Home");
    }

    // GET: Registar
    [HttpGet]
    public IActionResult Registar()
    {
        var model = new RegistarViewModel();
        return View(model);
    }

    // POST: Registar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Registar(RegistarViewModel model)
    {
        if (!ModelState.IsValid)
        {
            TempData["Erro"] = "Dados inválidos.";
            return View(model);
        }

        // Verifica se o email já existe
        var existingUser = await _context.Utilizadores.FirstOrDefaultAsync(u => u.Email == model.Email);
        if (existingUser != null)
        {
            TempData["Erro"] = "O email já está registado.";
            return View(model);
        }

        // Cria novo utilizador
        var novoUtilizador = new Utilizador
        {
            Nome = model.Nome,
            Email = model.Email,
            Password = model.Password, // Em produção, faz hash da password!
            Role = "Cliente"
        };

        _context.Utilizadores.Add(novoUtilizador);
        await _context.SaveChangesAsync();

        TempData["Sucesso"] = "Conta criada com sucesso!";
        return RedirectToAction("Login", new { email = model.Email });
    }
}
