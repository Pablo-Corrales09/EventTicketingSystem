using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Models;

namespace Web.Controllers;

public class EventosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public EventosController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // 1. GET: /Eventos
    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var eventos = await client.GetFromJsonAsync<List<EventoSedeViewModel>>("api/Eventos/localidad-sede");
            return View(eventos ?? new List<EventoSedeViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API de eventos.";
            return View(new List<EventoSedeViewModel>());
        }
    }

    // 2. GET: /Eventos/ObtenerDetalles/5
   [HttpGet]
    public async Task<IActionResult> ObtenerDetalles(int id)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var evento = await client.GetFromJsonAsync<EventoSedeViewModel>($"api/Eventos/{id}");
            
            if (evento == null) 
            {
                return NotFound("Evento no encontrado.");
            }

            try
            {
                var todasLasLocalidades = await client.GetFromJsonAsync<List<EventoLocalidadViewModel>>("api/EventosLocalidades");

                if (todasLasLocalidades != null)
                {
                    evento.Localidades = todasLasLocalidades.Where(l => l.IdEvento == id).ToList();
                }
                else
                {
                    evento.Localidades = new List<EventoLocalidadViewModel>();
                }
            }
            catch (HttpRequestException)
            {
                evento.Localidades = new List<EventoLocalidadViewModel>();
            }

            return Json(evento); 
        }
        catch (HttpRequestException)
        {
            return StatusCode(500, "Error de conexión con la API al buscar los detalles.");
        }
    }
}