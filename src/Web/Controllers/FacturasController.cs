using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class FacturasController : Controller
{
    private readonly IHttpClientFactory _httpClientFactory;

    public FacturasController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(string? numeroFactura, DateTime? fechaDesde, DateTime? fechaHasta, string? nombreEvento)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var esAdmin = AuthCookieHelper.EsAdmin(Request);
            ViewBag.EsAdmin = esAdmin;

            var parametros = new List<(string, object?)>
            {
                ("numeroFactura", numeroFactura),
                ("fechaDesde", fechaDesde),
                ("fechaHasta", fechaHasta),
                ("nombreEvento", nombreEvento)
            };

            if (!esAdmin)
            {
                var sesion = AuthCookieHelper.ObtenerSesion(Request);
                if (!sesion.Autenticado)
                {
                    ViewBag.Aviso = "Inicia sesión para ver tus facturas.";
                    return View(new List<FacturaViewModel>());
                }
                parametros.Add(("usuarioId", sesion.IdUsuario));
            }

            var query = QueryBuilder.Build(parametros.ToArray());
            var facturas = await client.GetFromJsonAsync<List<FacturaViewModel>>($"api/Facturas?{query}");

            return View(facturas ?? new List<FacturaViewModel>());
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible obtener las facturas desde la API.";
            return View(new List<FacturaViewModel>());
        }
    }
}
