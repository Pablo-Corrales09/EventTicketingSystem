using Microsoft.AspNetCore.Mvc;
using System.Net.Http.Json;
using Web.Helpers;
using Web.Models;

namespace Web.Controllers;

public class FacturasController : Controller
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    private readonly IHttpClientFactory _httpClientFactory;

    public FacturasController(IHttpClientFactory httpClientFactory)
    {
        _httpClientFactory = httpClientFactory;
    }

    public async Task<IActionResult> Index(string? numeroFactura, DateTime? fechaDesde, DateTime? fechaHasta, string? nombreEvento, string? reporteDesde, string? reporteHasta)
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

            var listaFacturas = facturas ?? new List<FacturaViewModel>();

            if (esAdmin)
            {
                var rango = ReporteHelper.ResolverRango(reporteDesde, reporteHasta);
                ViewBag.Reporte = ConstruirReporte(listaFacturas, rango.Desde, rango.Hasta);
                ViewBag.RangoReporte = ReporteHelper.FormatearPeriodo(rango.Desde, rango.Hasta);
                ViewBag.ReporteDesde = rango.Desde.ToString("yyyy-MM");
                ViewBag.ReporteHasta = rango.Hasta.ToString("yyyy-MM");
            }

            return View(listaFacturas);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible obtener las facturas desde la API.";
            return View(new List<FacturaViewModel>());
        }
    }

    // GET: /Facturas/Imprimir/{id}
    // Consume: api/Facturas/{id}
    public async Task<IActionResult> Imprimir(int id)
    {
        var client = _httpClientFactory.CreateClient("API");

        try
        {
            var factura = await client.GetFromJsonAsync<FacturaViewModel>($"api/Facturas/{id}");

            if (factura == null)
            {
                return NotFound();
            }

            return View(factura);
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible obtener la factura desde la API.";
            return RedirectToAction(nameof(Index));
        }
    }

    // GET: /Facturas/ExportarExcel
    // Consume: api/Facturas
    public async Task<IActionResult> ExportarExcel(string? numeroFactura, DateTime? fechaDesde, DateTime? fechaHasta, string? nombreEvento, string? reporteDesde, string? reporteHasta)
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
                ("numeroFactura", numeroFactura),
                ("fechaDesde", fechaDesde),
                ("fechaHasta", fechaHasta),
                ("nombreEvento", nombreEvento)
            };

            var query = QueryBuilder.Build(parametros.ToArray());
            var facturas = await client.GetFromJsonAsync<List<FacturaViewModel>>($"api/Facturas?{query}");

            var listaFacturas = facturas ?? new List<FacturaViewModel>();
            var rango = ReporteHelper.ResolverRango(reporteDesde, reporteHasta);
            var reporte = ConstruirReporte(listaFacturas, rango.Desde, rango.Hasta);

            var archivo = ReporteHelper.GenerarExcel(
                "Reporte de facturación",
                ReporteHelper.FormatearPeriodo(rango.Desde, rango.Hasta),
                new List<(string, object)>
                {
                    ("Total facturado", reporte.TotalMes),
                    ("Facturas emitidas", reporte.FacturasMes),
                    ("Boletos vendidos", reporte.BoletosMes)
                },
                reporte.Eventos);

            return File(archivo, TipoExcel, $"Reporte_Facturacion_{DateTime.Now:yyyyMMdd_HHmmss}.xlsx");
        }
        catch (HttpRequestException)
        {
            ViewBag.Error = "No fue posible obtener las facturas desde la API.";
            return RedirectToAction(nameof(Index));
        }
    }

    private static FacturasReporteViewModel ConstruirReporte(List<FacturaViewModel> facturas, DateOnly desde, DateOnly hasta)
    {
        var boletosEnRango = facturas
            .SelectMany(f => f.Boletos)
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

        return new FacturasReporteViewModel
        {
            Eventos = eventos,
            TotalMes = boletosEnRango.Sum(b => b.Precio),
            FacturasMes = boletosEnRango
                .Select(b => b.NumeroFactura)
                .Where(n => !string.IsNullOrWhiteSpace(n))
                .Distinct()
                .Count(),
            BoletosMes = boletosEnRango.Count
        };
    }
}