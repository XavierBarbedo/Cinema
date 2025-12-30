using Microsoft.EntityFrameworkCore;
using Cinema.Data;
using Cinema.Services; 

var builder = WebApplication.CreateBuilder(args);

// ==========================================
// Configuração de serviços
// ==========================================

// Liga o DbContext ao SQL Server LocalDB
builder.Services.AddDbContext<CinemaDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Adiciona suporte a controladores com views
builder.Services.AddControllersWithViews();

// Adicionar suporte a sessão
builder.Services.AddDistributedMemoryCache();  // Cache em memória para sessão
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30); // tempo de expiração da sessão
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// ==========================================
// Registar serviço TMDb
// ==========================================
builder.Services.AddHttpClient<TmdbService>();

var app = builder.Build();

// ==========================================
// Auto-Migration
// ==========================================
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<CinemaDbContext>();
        context.Database.Migrate();
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred creating the DB.");
    }
}

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
    pattern: "{controller=Home}/{action=Index}/{id?}");

app.Run();
