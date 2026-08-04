using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddOpenApi(); 

builder.Services.AddDbContext<DbDevTicketappContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<MedioPagoService>();
builder.Services.AddScoped<LocalidadService>();
builder.Services.AddScoped<EventoLocalidadService>();
builder.Services.AddScoped<BoletoService>();
builder.Services.AddScoped<FacturaService>();

var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        context.Response.ContentType = "application/json";

        var (statusCode, mensaje) = ex switch
        {
            // Errores de negocio como correo duplicado, sin boletos, etc. -> HTTP 400
            InvalidOperationException => (StatusCodes.Status400BadRequest, ex.Message),
            
          
            KeyNotFoundException => (StatusCodes.Status404NotFound, ex.Message),
            
            _ => (StatusCodes.Status500InternalServerError, "Ocurrió un error interno en el servidor.")
        };

        context.Response.StatusCode = statusCode;
        await context.Response.WriteAsJsonAsync(new { error = mensaje });
    }
});



if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.MapControllers(); 

app.Run();