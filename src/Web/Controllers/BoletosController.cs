using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class BoletosController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public BoletosController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Boletos
    // Consume: api/Boletos
    public async Task<IActionResult> Index(string? numeroBoleto, string? nombreEvento, string? nombreCliente)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var esAdmin = AuthCookieHelper.EsAdmin(Request);
            ViewBag.EsAdmin = esAdmin;

            var parametros = new List<(string, object?)>
            {
                ("numeroBoleto", numeroBoleto),
                ("nombreEvento", nombreEvento)
            };

            if (esAdmin)
            {
                parametros.Add(("nombreCliente", nombreCliente));
            }
            else
            {
                var sesion = AuthCookieHelper.ObtenerSesion(Request);
                if (!sesion.Autenticado)
                {
                    ViewBag.Aviso = "Inicia sesión para ver tus boletos.";
                    return View(new List<BoletoViewModel>());
                }
                parametros.Add(("usuarioId", sesion.IdUsuario));
            }

            var query = QueryBuilder.Build(parametros.ToArray());
            var boletos = await client.GetFromJsonAsync<List<BoletoViewModel>>($"api/Boletos?{query}");
            return View(boletos ?? new List<BoletoViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API.";
            return View(new List<BoletoViewModel>());
        }
    }

    // GET: /Boletos/Details/5
    // Consume: api/Boletos/{id}
    public async Task<IActionResult> Details(int id)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var boleto = await client.GetFromJsonAsync<BoletoViewModel>($"api/Boletos/{id}");
            
            if (boleto == null) 
                return NotFound();
                
            return View(boleto);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Boletos/Comprar
    // Muestra el formulario vacío para realizar la compra
    public IActionResult Comprar()
    {
        return View();
    }

    // POST: /Boletos/Comprar
    // Consume: api/Boletos/comprar
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Comprar(BoletoCreacionViewModel modelo)
    {
        if (!ModelState.IsValid)
        {
            return View(modelo);
        }

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var response = await client.PostAsJsonAsync("api/Boletos/comprar", modelo);

            if (response.IsSuccessStatusCode)
            {
                // Si la compra fue exitosa, redirige a los detalles del boleto
                var boletoCreado = await response.Content.ReadFromJsonAsync<BoletoViewModel>();
                return RedirectToAction(nameof(Details), new { id = boletoCreado?.IdBoleto });
            }
            else
            {
                ModelState.AddModelError(string.Empty, "Error al procesar la compra. Verifica los datos enviados.");
                return View(modelo);
            }
        }
        catch (HttpRequestException)
        {
            ModelState.AddModelError(string.Empty, "No fue posible conectarse con la API en este momento.");
            return View(modelo);
        }
    }
}