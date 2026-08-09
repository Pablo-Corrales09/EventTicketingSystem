using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class AuthController : Controller
{
    private const string TokenCookie = "AuthToken";
    private const string InfoCookie = "AuthInfo";

    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // POST: /Auth/Login
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Correo) || string.IsNullOrWhiteSpace(model.Contrasena))
        {
            return BadRequest(new { mensaje = "Debes ingresar el correo y la contraseña." });
        }

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var respuesta = await client.PostAsJsonAsync("api/Auth/login", new
            {
                correo = model.Correo,
                contrasena = model.Contrasena
            });

            var contenido = await respuesta.Content.ReadFromJsonAsync<ResultadoLoginApi>();

            if (contenido == null || string.IsNullOrWhiteSpace(contenido.Token))
            {
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
            }

            EstablecerCookies(model.Recordarme, contenido.Token, contenido.Nombre ?? string.Empty, contenido.Correo ?? string.Empty, contenido.Role);

            return Ok(new
            {
                mensaje = "Inicio de sesión exitoso.",
                nombre = contenido.Nombre,
                correo = contenido.Correo,
                role = contenido.Role
            });
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, new { mensaje = "No fue posible conectarse con el servidor de autenticación." });
        }
    }

    // POST: /Auth/Registrar
    [HttpPost]
    public async Task<IActionResult> Registrar([FromBody] UsuarioRegistroViewModel model)
    {
        if (string.IsNullOrWhiteSpace(model.Nombre) || string.IsNullOrWhiteSpace(model.Apellido) ||
            string.IsNullOrWhiteSpace(model.Correo) || string.IsNullOrWhiteSpace(model.Contrasena))
        {
            return BadRequest(new { mensaje = "Todos los campos son obligatorios." });
        }

        if (model.Contrasena.Length < 8)
        {
            return BadRequest(new { mensaje = "La contraseña debe tener al menos 8 caracteres." });
        }

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var respuesta = await client.PostAsJsonAsync("api/Usuarios/crearUsuario", new
            {
                nombre = model.Nombre,
                apellido = model.Apellido,
                telefono = model.Telefono,
                correo = model.Correo,
                contrasena = model.Contrasena
            });

            if (!respuesta.IsSuccessStatusCode)
            {
                var error = await respuesta.Content.ReadFromJsonAsync<ResultadoErrorApi>();
                return BadRequest(new { mensaje = error?.Error ?? "No se pudo registrar el usuario. Verifica los datos." });
            }

            return Ok(new { mensaje = "Registro exitoso. Ahora puedes iniciar sesión." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, new { mensaje = "No fue posible conectarse con el servidor." });
        }
    }

    // GET: /Auth/Estado
    [HttpGet]
    public IActionResult Estado()
    {
        var info = Request.Cookies[InfoCookie];

        if (string.IsNullOrWhiteSpace(info))
        {
            return Ok(new UsuarioSesionViewModel { Autenticado = false });
        }

        var partes = info.Split('|');

        return Ok(new UsuarioSesionViewModel
        {
            Autenticado = true,
            Nombre = partes.Length > 0 ? partes[0] : string.Empty,
            Correo = partes.Length > 1 ? partes[1] : string.Empty,
            Role = partes.Length > 2 ? partes[2] : null
        });
    }

    // POST: /Auth/Logout
    [HttpPost]
    public IActionResult Logout()
    {
        Response.Cookies.Delete(TokenCookie);
        Response.Cookies.Delete(InfoCookie);
        return Ok(new { mensaje = "Sesión cerrada." });
    }

    private void EstablecerCookies(bool recordarme, string token, string nombre, string correo, string? role)
    {
        var opciones = new CookieOptions
        {
            HttpOnly = true,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps
        };

        if (recordarme)
        {
            opciones.Expires = DateTimeOffset.UtcNow.AddDays(7);
        }

        Response.Cookies.Append(TokenCookie, token, opciones);

        Response.Cookies.Append(InfoCookie, $"{nombre}|{correo}|{role}", new CookieOptions
        {
            HttpOnly = false,
            SameSite = SameSiteMode.Lax,
            Secure = Request.IsHttps,
            Expires = opciones.Expires
        });
    }

    private sealed class ResultadoLoginApi
    {
        public string? Token { get; set; }
        public string? Nombre { get; set; }
        public string? Correo { get; set; }
        public string? Role { get; set; }
    }

    private sealed class ResultadoErrorApi
    {
        public string? Error { get; set; }
    }
}
