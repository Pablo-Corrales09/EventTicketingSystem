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

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var eventos = await client.GetFromJsonAsync<List<EventoViewModel>>(
                "api/Eventos/localidad-sede"
            );

            return View(eventos ?? new List<EventoViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API.";
            return View(new List<EventoViewModel>());
        }
    }
}