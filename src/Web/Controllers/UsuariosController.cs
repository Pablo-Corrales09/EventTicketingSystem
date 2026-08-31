using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class UsuariosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public UsuariosController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    private HttpClient CrearClienteApi()
    {
        return _httpClientFactory.CreateClient("API");
    }

    private bool EsAdmin()
    {
        return AuthCookieHelper.EsAdmin(Request);
    }

    // GET: /Usuarios
    public async Task<IActionResult> Index()
    {
        if (!EsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.EsAdmin = true;

        var client = CrearClienteApi();

        try
        {
            var usuarios = await client.GetFromJsonAsync<List<UsuarioViewModel>>("api/Usuarios");
            return View(usuarios ?? new List<UsuarioViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API de usuarios.";
            return View(new List<UsuarioViewModel>());
        }
    }

    // GET: /Usuarios/Editar/5
    public async Task<IActionResult> Editar(int id)
    {
        if (!EsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.EsAdmin = true;

        var client = CrearClienteApi();

        try
        {
            var usuario = await client.GetFromJsonAsync<UsuarioViewModel>($"api/Usuarios/{id}");

            if (usuario == null)
            {
                return NotFound("Usuario no encontrado.");
            }

            var modelo = new UsuarioEdicionViewModel
            {
                IdUsuario = usuario.IdUsuario,
                Nombre = usuario.Nombre,
                Apellido = usuario.Apellido,
                Correo = usuario.Correo,
                Telefono = usuario.Telefono,
                IdRole = usuario.IdRole
            };

            await CargarRolesAsync(modelo);
            return View(modelo);
        }
        catch (HttpRequestException)
        {
            return RedirectToAction(nameof(Index));
        }
    }

    // POST: /Usuarios/Editar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Editar(int id, UsuarioEdicionViewModel modelo)
    {
        if (!EsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        ViewBag.EsAdmin = true;

        if (!ModelState.IsValid)
        {
            await CargarRolesAsync(modelo);
            return View(modelo);
        }

        var client = CrearClienteApi();

        try
        {
            var respuesta = await client.PutAsJsonAsync($"api/Usuarios/actualizar/{id}", new
            {
                nombre = modelo.Nombre,
                apellido = modelo.Apellido,
                correo = modelo.Correo,
                telefono = modelo.Telefono,
                idRole = modelo.IdRole
            });

            if (respuesta.IsSuccessStatusCode)
            {
                return RedirectToAction(nameof(Index));
            }

            var mensajeError = await LeerMensajeErrorAsync(respuesta);
            ModelState.AddModelError(string.Empty, mensajeError ?? "No se pudo actualizar el usuario. Verifica los datos.");
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "No fue posible conectarse con la API en este momento.");
        }

        await CargarRolesAsync(modelo);
        return View(modelo);
    }

    // POST: /Usuarios/Desactivar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Desactivar(int id)
    {
        if (!EsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        var client = CrearClienteApi();

        try
        {
            var respuesta = await client.PutAsync($"api/Usuarios/desactivar/{id}", null);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "El usuario fue desactivado correctamente.";
            }
            else
            {
                var mensajeError = await LeerMensajeErrorAsync(respuesta);
                TempData["Error"] = mensajeError ?? "No se pudo desactivar el usuario. Inténtalo de nuevo.";
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "No fue posible conectarse con la API en este momento.";
        }

        return RedirectToAction(nameof(Index));
    }

    // POST: /Usuarios/Activar/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Activar(int id)
    {
        if (!EsAdmin())
        {
            return RedirectToAction("Index", "Home");
        }

        var client = CrearClienteApi();

        try
        {
            var respuesta = await client.PutAsync($"api/Usuarios/activar/{id}", null);

            if (respuesta.IsSuccessStatusCode)
            {
                TempData["Exito"] = "El usuario fue activado correctamente.";
            }
            else
            {
                var mensajeError = await LeerMensajeErrorAsync(respuesta);
                TempData["Error"] = mensajeError ?? "No se pudo activar el usuario. Inténtalo de nuevo.";
            }
        }
        catch (HttpRequestException)
        {
            TempData["Error"] = "No fue posible conectarse con la API en este momento.";
        }

        return RedirectToAction(nameof(Index));
    }

    private async Task CargarRolesAsync(UsuarioEdicionViewModel modelo)
    {
        try
        {
            var client = CrearClienteApi();
            var roles = await client.GetFromJsonAsync<List<RoleViewModel>>("api/Usuarios/roles");
            modelo.Roles = roles ?? new List<RoleViewModel>();
        }
        catch (HttpRequestException)
        {
            modelo.Roles = new List<RoleViewModel>();
        }
    }

    private static async Task<string?> LeerMensajeErrorAsync(HttpResponseMessage respuesta)
    {
        try
        {
            var error = await respuesta.Content.ReadFromJsonAsync<ResultadoErrorApi>();
            return error?.Error ?? error?.Mensaje;
        }
        catch
        {
            return null;
        }
    }

    private sealed class ResultadoErrorApi
    {
        public string? Error { get; set; }
        public string? Mensaje { get; set; }
    }
}