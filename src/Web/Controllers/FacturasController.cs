using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Models;

namespace Web.Controllers;

public class FacturasController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FacturasController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index()
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var facturas = await client.GetFromJsonAsync<List<FacturaViewModel>>(
                "api/Facturas"
            );

            return View(facturas ?? new List<FacturaViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible obtener las facturas desde la API.";
            return View(new List<FacturaViewModel>());
        }
    }
}