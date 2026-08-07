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
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var todasLocalidades = await client.GetFromJsonAsync<List<EventoLocalidadViewModel>>("api/EventosLocalidades");
            var localidadSeleccionada = todasLocalidades?.FirstOrDefault(l => l.IdEventoLocalidad == idEventoLocalidad);

            if (localidadSeleccionada == null)
            {
                return NotFound("La localidad seleccionada no es válida o ya no está disponible.");
            }

            var modelo = new ProcesoPagoViewModel
            {
                IdEventoLocalidad = localidadSeleccionada.IdEventoLocalidad,
                NombreEvento = localidadSeleccionada.NombreEvento,
                NombreLocalidad = localidadSeleccionada.NombreLocalidad,
                Precio = localidadSeleccionada.Precio,
                IdMedioPago = 1
            };

            return View(modelo);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectar con la API para procesar la compra.";
            return View(new ProcesoPagoViewModel());
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcesarPago(ProcesoPagoViewModel modelo)
    {
        var client = _httpClientFactory.CreateClient("API");

        var facturaRequest = new
        {
            IdUsuario = modelo.IdUsuario,
            IdMedioPago = modelo.IdMedioPago,
            IdEventoLocalidad = modelo.IdEventoLocalidad
        };

        try
        {
            var response = await client.PostAsJsonAsync("api/Facturas/crear", facturaRequest);

            if (response.IsSuccessStatusCode)
            {
                var facturaCreada = await response.Content.ReadFromJsonAsync<FacturaResponseDto>();
                return RedirectToAction(nameof(Confirmacion), new { id = facturaCreada?.IdFactura });
            }

            ModelState.AddModelError(string.Empty, "Hubo un error al procesar el pago y generar la factura.");
            return View(modelo);
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "Error de comunicación con la API de facturación.");
            return View(modelo);
        }
    }

    public async Task<IActionResult> Confirmacion(int id)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var factura = await client.GetFromJsonAsync<FacturaResponseDto>($"api/Facturas/{id}");
            return View(factura);
        }
        catch (HttpRequestException)
        {
            return View();
        }
    }
}