using Microsoft.EntityFrameworkCore;
using PiscAtlas.Models; 

var builder = WebApplication.CreateBuilder(args);

// --- LIGAÇÃO À BASE DE DADOS ---
// Dizemos à API como se ligar ao SQL Server usando a string do appsettings.json
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add services to the container.
builder.Services.AddControllers().AddJsonOptions(options =>
{
    // Ignora os loops infinitos nas relações das tabelas
    options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
});


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    // Ativa a interface gráfica do Swagger para testar a API
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();