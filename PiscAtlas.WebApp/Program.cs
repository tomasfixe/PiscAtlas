using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models;
using PiscAtlas.Models.Models;
using System.Linq;
var builder = WebApplication.CreateBuilder(args);

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

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

// Garante que utilizadores nao autenticados (ou banidos) sao
// redirecionados para a pagina de login correta da aplicacao.
builder.Services.ConfigureApplicationCookie(options =>
{
    options.LoginPath = "/Conta/Login";
    options.AccessDeniedPath = "/Conta/Login";
});

// Add services to the container.
builder.Services.AddRazorPages();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseStaticFiles();

app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapRazorPages();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

using (var scope = app.Services.CreateScope())
{
    var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = scope.ServiceProvider.GetRequiredService<UserManager<Utilizador>>();

    // Garante que o cargo Admin existe
    if (!await roleManager.RoleExistsAsync("Admin"))
    {
        await roleManager.CreateAsync(new IdentityRole("Admin"));
    }

    // --- Conta de Admin de teste ---
    // Cria automaticamente uma conta de admin para testes, caso ainda nao exista.
    // ATENCAO: alterar a password / remover este bloco antes de ir para producao.
    const string adminEmail = "admin@piscatlas.pt";
    const string adminPassword = "123456";

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new Utilizador
        {
            UserName       = adminEmail,
            Email          = adminEmail,
            EmailConfirmed = true,
            PrimeiroNome   = "Admin",
            UltimoNome     = "PiscAtlas",
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

    // Da tambem o cargo Admin ao teu email pessoal, se essa conta ja existir
    var user = await userManager.FindByEmailAsync("ambmatos193@gmail.com"); // MUDAR AQUI
    if (user != null && !await userManager.IsInRoleAsync(user, "Admin"))
    {
        await userManager.AddToRoleAsync(user, "Admin");
    }
}

app.Run();
