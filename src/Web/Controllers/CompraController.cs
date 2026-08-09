using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Models;

namespace Web.Controllers;

public class CompraController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public CompraController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    [HttpGet]
    public async Task<IActionResult> Comprar(int idEventoLocalidad)
    {
        var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

        if (idUsuario == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var todasLocalidades =
                await client.GetFromJsonAsync<List<EventoLocalidadViewModel>>(
                    "api/EventosLocalidades"
                );

            var localidadSeleccionada = todasLocalidades?
                .FirstOrDefault(l =>
                    l.IdEventoLocalidad == idEventoLocalidad
                );

            if (localidadSeleccionada == null)
            {
                return NotFound(
                    "La localidad seleccionada no es válida o ya no está disponible."
                );
            }

            var modelo = new ProcesoPagoViewModel
            {
                IdEventoLocalidad =
                    localidadSeleccionada.IdEventoLocalidad,

                NombreEvento =
                    localidadSeleccionada.NombreEvento,

                NombreLocalidad =
                    localidadSeleccionada.NombreLocalidad,

                Precio =
                    localidadSeleccionada.Precio,

                IdUsuario = idUsuario.Value,

                IdMedioPago = 1
            };

            return View(modelo);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error =
                "No fue posible conectar con la API para procesar la compra.";

            return View(new ProcesoPagoViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarPago(
        ProcesoPagoViewModel modelo
    )
    {
        var idUsuario = HttpContext.Session.GetInt32("IdUsuario");

        if (idUsuario == null)
        {
            return RedirectToAction("Login", "Auth");
        }

        var client = _httpClientFactory.CreateClient("API");

        var compraRequest = new
        {
            IdEventoLocalidad = modelo.IdEventoLocalidad,
            IdUsuario = idUsuario.Value,
            IdMedioPago = modelo.IdMedioPago
        };

        try
        {
            var response = await client.PostAsJsonAsync(
                "api/Boletos/comprar",
                compraRequest
            );

            if (response.IsSuccessStatusCode)
            {
                var boletoCreado =
                    await response.Content
                        .ReadFromJsonAsync<BoletoViewModel>();

                if (boletoCreado == null)
                {
                    ModelState.AddModelError(
                        string.Empty,
                        "La compra fue procesada, pero no fue posible obtener la información del boleto."
                    );

                    return View("Comprar", modelo);
                }

                return RedirectToAction(
                    "Details",
                    "Boletos",
                    new { id = boletoCreado.IdBoleto }
                );
            }

            ModelState.AddModelError(
                string.Empty,
                "Hubo un error al procesar la compra."
            );

            return View("Comprar", modelo);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(
                string.Empty,
                "No fue posible comunicarse con la API."
            );

            return View("Comprar", modelo);
        }
    }
}