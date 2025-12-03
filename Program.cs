using Microsoft.EntityFrameworkCore;
using Cinema.Data;

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Configuração de serviços
// ==========================================

// Liga o DbContext ao SQL Server LocalDB
builder.Services.AddDbContext<CinemaDbContext>(options =>
options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona suporte a controladores com views
builder.Services.AddControllersWithViews();

//  Adicionar suporte a sessão
builder.Services.AddDistributedMemoryCache();  // Cache em memória para sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

var app = builder.Build();

// ==========================================
// Middleware
// ==========================================

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

// Ativar sessão antes de UseAuthorization
app.UseSession();

app.UseAuthorization();

// Rota padrão
app.MapControllerRoute(
name: "default",
pattern: "{controller=Utilizador}/{action=Login}/{id?}");

app.Run();
