using System.Net.Http.Headers;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Web.Models;

namespace Web.Controllers;

public class CompraController : Controller
{
    private const string InfoCookie = "AuthInfo";
    private const string TokenCookie = "AuthToken";

    private readonly IHttpClientFactory _httpClientFactory;

    public CompraController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Compra/Comprar?idEventoLocalidad=5&cantidad=2
    public async Task<IActionResult> Comprar(int idEventoLocalidad, int cantidad = 1)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var todasLocalidades = await client.GetFromJsonAsync<List<EventoLocalidadViewModel>>("api/EventosLocalidades");
            var localidadSeleccionada = todasLocalidades?.FirstOrDefault(l => l.IdEventoLocalidad == idEventoLocalidad);

            if (localidadSeleccionada == null)
            {
                return NotFound("La localidad seleccionada no es válida o ya no está disponible.");
            }

            var mediosPago = await client.GetFromJsonAsync<List<MedioPagoViewModel>>("api/MediosPago");
            ViewBag.MediosPago = mediosPago ?? new List<MedioPagoViewModel>();

            cantidad = Math.Clamp(cantidad, 1, Math.Max(1, localidadSeleccionada.CapacidadDisponible));

            var modelo = new ProcesoPagoViewModel
            {
                IdEvento = localidadSeleccionada.IdEvento,
                IdEventoLocalidad = localidadSeleccionada.IdEventoLocalidad,
                NombreEvento = localidadSeleccionada.NombreEvento,
                NombreLocalidad = localidadSeleccionada.NombreLocalidad,
                Precio = localidadSeleccionada.Precio,
                Cantidad = cantidad,
                IdMedioPago = 0,
                IdUsuario = ObtenerIdUsuarioSesion() ?? 0
            };

            return View(modelo);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectar con la API para procesar la compra.";
            ViewBag.MediosPago = new List<MedioPagoViewModel>();
            return View(new ProcesoPagoViewModel());
        }
    }

    // POST: /Compra/ProcesarPago
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarPago([FromBody] ProcesoPagoViewModel modelo)
    {
        var idUsuario = ObtenerIdUsuarioSesion();
        if (idUsuario == null)
        {
            return Ok(new { requiereLogin = true, mensaje = "Debes iniciar sesión para completar la compra." });
        }

        if (modelo.IdMedioPago <= 0 || modelo.IdMedioPago > 3)
        {
            return BadRequest(new { mensaje = "Seleccione un medio de pago válido." });
        }

        var token = Request.Cookies[TokenCookie];
        var client = _httpClientFactory.CreateClient("API");

        var request = new HttpRequestMessage(HttpMethod.Post, "api/Boletos/comprar")
        {
            Content = JsonContent.Create(new
            {
                idEventoLocalidad = modelo.IdEventoLocalidad,
                idUsuario = idUsuario.Value,
                idMedioPago = modelo.IdMedioPago,
                cantidad = Math.Max(1, modelo.Cantidad)
            })
        };

        if (!string.IsNullOrEmpty(token))
        {
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        }

        try
        {
            var response = await client.SendAsync(request);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return Ok(new { requiereLogin = true, mensaje = "Tu sesión ha expirado. Inicia sesión para continuar." });
            }

            if (response.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            {
                return StatusCode(429, new { mensaje = "Has superado el límite de compras permitidas. Espera un momento e inténtalo de nuevo." });
            }

            if (response.IsSuccessStatusCode)
            {
                var boletoCreado = await response.Content.ReadFromJsonAsync<BoletoViewModel>();
                return Ok(new { exito = true, idBoleto = boletoCreado?.IdBoleto });
            }

            var mensajeError = await LeerMensajeErrorAsync(response);
            return BadRequest(new { mensaje = mensajeError ?? "Hubo un error al procesar el pago. Inténtalo de nuevo." });
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, new { mensaje = "Error de comunicación con la API de compra." });
        }
    }

    private int? ObtenerIdUsuarioSesion()
    {
        var info = Request.Cookies[InfoCookie];
        if (string.IsNullOrWhiteSpace(info))
        {
            return null;
        }

        var partes = info.Split('|');

        // Un idUsuario <= 0 (cookie vieja o sesión sin id) no se considera autenticado.
        if (partes.Length == 0 || !int.TryParse(partes[0], out var id) || id <= 0)
        {
            return null;
        }

        return id;
    }

    private static async Task<string?> LeerMensajeErrorAsync(HttpResponseMessage response)
    {
        try
        {
            var error = await response.Content.ReadFromJsonAsync<ResultadoErrorApi>();
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
