using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PokeTorneio.Data;
using PokeTorneio.Services;

var builder = WebApplication.CreateBuilder(args);

// Configura a string de conexão com SQLite
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

// Log para depuração
Console.WriteLine($"Connection String: {connectionString}");

// Adiciona o contexto do banco de dados usando SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(connectionString));

// Configuração do Identity com o ApplicationDbContext
builder.Services.AddDefaultIdentity<IdentityUser>(options =>
{
    options.SignIn.RequireConfirmedAccount = true; // Requer confirmação de conta
})
.AddEntityFrameworkStores<ApplicationDbContext>();

// Registra os serviços necessários
builder.Services.AddScoped<ITorneioService, TorneioService>();

// Adiciona suporte a controladores e views
builder.Services.AddControllersWithViews();

// Cria a aplicação
var app = builder.Build();

// Configuração do middleware
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts(); // HSTS (HTTP Strict Transport Security)
}

app.UseHttpsRedirection(); // Redireciona HTTP para HTTPS
app.UseStaticFiles(); // Permite servir arquivos estáticos
app.UseRouting(); // Habilita o roteamento
app.UseAuthentication(); // Habilita a autenticação
app.UseAuthorization(); // Habilita a autorização

// Inicializa o banco de dados
using (var scope = app.Services.CreateScope())
{
    var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
    dbContext.Database.EnsureCreated(); // Cria o banco de dados se não existir
}

// Configuração das rotas
app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");
app.MapRazorPages(); // Mapear páginas Razor

// Inicia a aplicação
app.Run();
