using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class BoletosController : Controller
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IHttpClientFactory _httpClientFactory;

    public BoletosController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    // GET: /Boletos
    // Consume: api/Boletos
    public async Task<IActionResult> Index(string? numeroBoleto, string? nombreEvento, string? nombreCliente, string? reporteDesde, string? reporteHasta)
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

            var listaBoletos = boletos ?? new List<BoletoViewModel>();

            if (esAdmin)
            {
                var rango = ReporteHelper.ResolverRango(reporteDesde, reporteHasta);
                ViewBag.Reporte = ConstruirReporte(listaBoletos, rango.Desde, rango.Hasta);
                ViewBag.RangoReporte = ReporteHelper.FormatearPeriodo(rango.Desde, rango.Hasta);
                ViewBag.ReporteDesde = rango.Desde.ToString("yyyy-MM");
                ViewBag.ReporteHasta = rango.Hasta.ToString("yyyy-MM");
            }

            return View(listaBoletos);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API.";
            return View(new List<BoletoViewModel>());
        }
    }

    // GET: /Boletos/ExportarExcel
    // Consume: api/Boletos
    public async Task<IActionResult> ExportarExcel(string? numeroBoleto, string? nombreEvento, string? nombreCliente, string? reporteDesde, string? reporteHasta)
    {
        if (!AuthCookieHelper.EsAdmin(Request))
        {
            return RedirectToAction(nameof(Index));
        }

        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var parametros = new List<(string, object?)>
            {
                ("numeroBoleto", numeroBoleto),
                ("nombreEvento", nombreEvento),
                ("nombreCliente", nombreCliente)
            };

            var query = QueryBuilder.Build(parametros.ToArray());
            var boletos = await client.GetFromJsonAsync<List<BoletoViewModel>>($"api/Boletos?{query}");

            var listaBoletos = boletos ?? new List<BoletoViewModel>();
            var rango = ReporteHelper.ResolverRango(reporteDesde, reporteHasta);
            var reporte = ConstruirReporte(listaBoletos, rango.Desde, rango.Hasta);

            var archivo = ReporteHelper.GenerarExcel(
                "Reporte de ventas de boletos",
                ReporteHelper.FormatearPeriodo(rango.Desde, rango.Hasta),
                new List<(string, object)>
                {
                    ("Total facturado", reporte.TotalMes),
                    ("Boletos vendidos", reporte.BoletosMes),
                    ("Eventos con ventas", reporte.EventosMes)
                },
                reporte.Eventos);

            return File(archivo, TipoExcel, $"Reporte_Boletos_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible conectarse con la API.";
            return RedirectToAction(nameof(Index));
        }
    }

    private static BoletosReporteViewModel ConstruirReporte(List<BoletoViewModel> boletos, DateOnly desde, DateOnly hasta)
    {
        var boletosEnRango = boletos
            .Where(b => b.FechaCompra.HasValue
                        && b.FechaCompra.Value >= desde
                        && b.FechaCompra.Value <= hasta)
            .ToList();

        var eventos = boletosEnRango
            .GroupBy(b => b.NombreEvento)
            .Where(g => !string.IsNullOrWhiteSpace(g.Key))
            .Select(g => new ReporteEventoViewModel
            {
                NombreEvento = g.Key,
                CantidadBoletos = g.Count(),
                TotalFacturado = g.Sum(b => b.Precio)
            })
            .OrderByDescending(e => e.TotalFacturado)
            .ToList();

        return new BoletosReporteViewModel
        {
            Eventos = eventos,
            TotalMes = boletosEnRango.Sum(b => b.Precio),
            BoletosMes = boletosEnRango.Count,
            EventosMes = boletosEnRango
                .Select(b => b.NombreEvento)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .Count()
        };
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

}