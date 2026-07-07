using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using System.Linq;
using PiscAtlas.WebApp.Hubs;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuração da Base de Dados
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// 2. Configuração do Identity (Autenticação)
builder.Services.AddIdentity<Utilizador, IdentityRole>(options =>
{
    // Regras das passwords (simplificadas para ser mais fácil testar)
    options.Password.RequireDigit = false;
    options.Password.RequiredLength = 6;
    options.Password.RequireNonAlphanumeric = false;
    options.Password.RequireUppercase = false;
    options.Password.RequireLowercase = false;
})
.AddEntityFrameworkStores<ApplicationDbContext>()
.AddDefaultTokenProviders();

// Garante que utilizadores nao autenticados sao redirecionados para a pagina de login correta
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.AccessDeniedPath = "/Conta/Login";
});

// 3. Adicionar Serviços Core (Apenas Razor Pages e SignalR)
builder.Services.AddRazorPages();
builder.Services.AddSignalR();

var app = builder.Build();

// 4. Configurar o HTTP request pipeline
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Erro");
    // The default HSTS value is 30 days. You may want to change this for production scenarios.
    app.UseHsts();
}

// Aponta erros 404, 403, etc., para a nossa página de erro customizada
app.UseStatusCodePagesWithReExecute("/Home/Erro", "?statusCode={0}");

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

// 5. Mapeamento de Rotas
app.MapRazorPages();
app.MapHub<NotificacaoHub>("/notificacaoHub");

// Redireciona a raiz ("/") automaticamente para o teu Index da Home
app.MapGet("/", context => {
    context.Response.Redirect("/Home/Index");
    return Task.CompletedTask;
});

// 6. Seeding de Dados (Criação do Admin)
using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Utilizador>>();

    // Garante que o cargo Admin existe
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // Cria automaticamente uma conta de admin para testes
    const string adminEmail = "admin@piscatlas.pt";
    const string adminPassword = "123456";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new Utilizador
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true,
            PrimeiroNome = "Admin",
            UltimoNome = "PiscAtlas",
            NomeUtilizador = "admin"
        };

        var criarResult = await userManager.CreateAsync(adminUser, adminPassword);
        if (!criarResult.Succeeded)
        {
            var erros = string.Join("; ", criarResult.Errors.Select(e => e.Description));
            throw new Exception($"Nao foi possivel criar a conta de admin de teste: {erros}");
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, "Admin"))
    {
        await userManager.AddToRoleAsync(adminUser, "Admin");
    }

    // Da tambem o cargo Admin ao teu email pessoal
    var user = await userManager.FindByEmailAsync("ambmatos193@gmail.com");
    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

app.Run();