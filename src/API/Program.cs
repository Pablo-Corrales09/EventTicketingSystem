using System.Text;
using System.Security.Claims;
using System.Threading.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using API.Data;
using API.Services;
using Microsoft.IdentityModel.Tokens;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers(); 
builder.Services.AddOpenApi(); 

builder.Services.AddDbContext<DbDevTicketappContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddScoped<UserService>();
builder.Services.AddScoped<EventoService>();
builder.Services.AddScoped<SedeEventoService>();
builder.Services.AddScoped<MedioPagoService>();
builder.Services.AddScoped<LocalidadService>();
builder.Services.AddScoped<EventoLocalidadService>();
builder.Services.AddScoped<BoletoService>();
builder.Services.AddScoped<FacturaService>();
builder.Services.AddScoped<LoginService>();
builder.Services.AddScoped<TokenService>();

var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["Secret"] ?? throw new InvalidOperationException("JwtSettings:Secret no esta configurado.");

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtSettings["Issuer"],
        ValidAudience = jwtSettings["Audience"],
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(secretKey))
    };
});

builder.Services.AddAuthorization();

//Limita la cantidad de peticiones de compra por usuario para prevenir compras automatizadas (bots).
builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

    options.AddPolicy("CompraUsuario", context =>
    {
        var identificador = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
        var clave = string.IsNullOrEmpty(identificador)
            ? context.Connection.RemoteIpAddress?.ToString() ?? "anónimo"
            : identificador;

        return RateLimitPartition.GetFixedWindowLimiter(clave, _ =>
            new FixedWindowRateLimiterOptions
            {
                PermitLimit = builder.Configuration.GetValue<int>("RateLimits:CompraPorUsuario:PermitLimit", 5),
                Window = TimeSpan.FromSeconds(builder.Configuration.GetValue<int>("RateLimits:CompraPorUsuario:WindowSegundos", 60)),
                QueueLimit = 0
            });
    });
});


builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirAngular",
        policy => policy.WithOrigins("http://localhost:4200")
                        .AllowAnyHeader()
                        .AllowAnyMethod());
});




var app = builder.Build();

app.Use(async (context, next) =>
{
    try
    {
        await next();
    }
    catch (Exception ex)
    {
        app.Logger.LogError(ex, "Excepción no controlada en la API en la ruta {Ruta}.", context.Request.Path);
        context.Response.ContentType = "application/json";

        var (statusCode, mensaje) = ex switch
        {
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
app.UseCors("PermitirAngular");

app.UseRateLimiter();

app.UseAuthentication(); 
app.UseAuthorization();  

app.MapControllers(); 

app.Run();