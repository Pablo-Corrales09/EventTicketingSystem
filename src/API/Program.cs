using Microsoft.EntityFrameworkCore;
using API.Data; // Ajusta según el namespace real de tu carpeta Data

var builder = WebApplication.CreateBuilder(args);

// 1. Agregar servicios al contenedor
builder.Services.AddControllers(); // Habilita el uso de controladores
builder.Services.AddOpenApi(); // Esto mantiene el soporte de OpenAPI que ya tenías

// 2. Registrar el DbContext con la cadena de conexión de tus secrets
builder.Services.AddDbContext<DbDevTicketappContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

var app = builder.Build();

// 3. Configurar el pipeline
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

// 4. Mapear los controladores
app.MapControllers(); 

app.Run();