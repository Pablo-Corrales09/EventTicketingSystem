using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Models;

namespace Web.Controllers;

public class AuthController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public AuthController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public IActionResult Login()
    {
        return View();
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var response = await client.PostAsJsonAsync(
                "api/Auth/login",
                modelo
            );

            if (!response.IsSuccessStatusCode)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "Correo o contraseña incorrectos."
                );

                return View(modelo);
            }

            var resultado = await response.Content
                .ReadFromJsonAsync<LoginResponseViewModel>();

            if (resultado == null ||
                string.IsNullOrWhiteSpace(resultado.Token))
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible completar el inicio de sesión."
                );

                return View(modelo);
            }

            var correo = Uri.EscapeDataString(modelo.Correo);

            var usuario = await client
                .GetFromJsonAsync<UsuarioSesionViewModel>(
                    $"api/Usuarios/buscarCorreo?correo={correo}"
                );

            if (usuario == null)
            {
                ModelState.AddModelError(
                    string.Empty,
                    "No fue posible obtener la información del usuario."
                );

                return View(modelo);
            }

            HttpContext.Session.SetInt32(
                "IdUsuario",
                usuario.IdUsuario
            );

            HttpContext.Session.SetString(
                "NombreUsuario",
                $"{usuario.Nombre} {usuario.Apellido}"
            );

            HttpContext.Session.SetString(
                "CorreoUsuario",
                usuario.Correo
            );

            HttpContext.Session.SetString(
                "Token",
                resultado.Token
            );

            return RedirectToAction(
                "Index",
                "Home"
            );
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No fue posible conectarse con la API."
            );

            return View(modelo);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public IActionResult Logout()
    {
        HttpContext.Session.Clear();

        return RedirectToAction(
            "Index",
            "Home"
        );
    }
}