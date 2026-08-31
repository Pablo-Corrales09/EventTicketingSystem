using System.Diagnostics;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class HomeController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;
    private readonly IMemoryCache _memoryCache;

    public HomeController(IHttpClientFactory httpClientFactory, IMemoryCache memoryCache)
    {
        _httpClientFactory = httpClientFactory;
        _memoryCache = memoryCache;
    }

    public async Task<IActionResult> Index()
    {
        ViewBag.EsAdmin = AuthCookieHelper.EsAdmin(Request);

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var eventos = await client.GetFromJsonAsync<List<EventoViewModel>>("api/Eventos/localidad-sede")
                          ?? new List<EventoViewModel>();

            var ordenHero = _memoryCache.GetOrCreate("heroOrdenEventos", entrada =>
            {
                entrada.AbsoluteExpirationRelativeToNow = TimeSpan.FromHours(6);
                return eventos
                    .Select(e => e.IdEvento)
                    .OrderBy(_ => Random.Shared.Next())
                    .Take(5)
                    .ToList();
            });

            ViewBag.OrdenHero = ordenHero;

            return View(eventos);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API de eventos.";
            return View(new List<EventoViewModel>());
        }
    }

    public IActionResult Privacy()
    {
        ViewBag.EsAdmin = AuthCookieHelper.EsAdmin(Request);
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
