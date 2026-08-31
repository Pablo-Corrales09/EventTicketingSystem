using System.Globalization;
using ClosedXML.Excel;
using Web.Models;

namespace Web.Helpers;

public static class ReporteHelper
{
    public static (DateOnly Desde, DateOnly Hasta) ResolverRango(string? reporteDesde, string? reporteHasta)
    {
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        var primerDiaMesActual = new DateOnly(hoy.Year, hoy.Month, 1);

        var desdeMes = ParseMes(reporteDesde) ?? primerDiaMesActual;
        var hastaMes = ParseMes(reporteHasta) ?? primerDiaMesActual;

        if (hastaMes < desdeMes)
        {
            (desdeMes, hastaMes) = (hastaMes, desdeMes);
        }

        var desde = new DateOnly(desdeMes.Year, desdeMes.Month, 1);
        var hasta = new DateOnly(
            hastaMes.Year,
            hastaMes.Month,
            DateTime.DaysInMonth(hastaMes.Year, hastaMes.Month));

        return (desde, hasta);
    }

    public static string FormatearPeriodo(DateOnly desde, DateOnly hasta)
    {
        if (desde.Year == hasta.Year && desde.Month == hasta.Month)
        {
            return desde.ToString("MMMM yyyy");
        }

        return $"{desde.ToString("MMMM yyyy")} - {hasta.ToString("MMMM yyyy")}";
    }

    public static byte[] GenerarExcel(
        string titulo,
        string periodo,
        IReadOnlyList<(string Etiqueta, object Valor)> resumen,
        IReadOnlyList<ReporteEventoViewModel> eventos)
    {
        using var wb = new XLWorkbook();
        var ws = wb.Worksheets.Add("Reporte");

        ws.Cell(1, 1).Value = titulo;
        ws.Cell(1, 1).Style.Font.Bold = true;
        ws.Cell(1, 1).Style.Font.FontSize = 14;

        ws.Cell(2, 1).Value = $"Período: {periodo}";
        ws.Cell(2, 1).Style.Font.Italic = true;

        int fila = 4;
        foreach (var item in resumen)
        {
            ws.Cell(fila, 1).Value = item.Etiqueta;
            ws.Cell(fila, 1).Style.Font.Bold = true;
            switch (item.Valor)
            {
                case decimal monto:
                    ws.Cell(fila, 2).Value = monto;
                    ws.Cell(fila, 2).Style.NumberFormat.Format = "#,##0.00";
                    break;
                case int entero:
                    ws.Cell(fila, 2).Value = entero;
                    break;
                case string texto:
                    ws.Cell(fila, 2).Value = texto;
                    break;
            }
            fila++;
        }

        fila += 1;

        ws.Cell(fila, 1).Value = "Evento";
        ws.Cell(fila, 2).Value = "Boletos";
        ws.Cell(fila, 3).Value = "Total";
        var rangoHeader = ws.Range(fila, 1, fila, 3);
        rangoHeader.Style.Font.Bold = true;
        rangoHeader.Style.Fill.BackgroundColor = XLColor.FromHtml("#0d6efd");
        rangoHeader.Style.Font.FontColor = XLColor.White;
        fila++;

        foreach (var e in eventos)
        {
            ws.Cell(fila, 1).Value = e.NombreEvento;
            ws.Cell(fila, 2).Value = e.CantidadBoletos;
            ws.Cell(fila, 3).Value = e.TotalFacturado;
            ws.Cell(fila, 3).Style.NumberFormat.Format = "#,##0.00";
            fila++;
        }

        ws.Columns(1, 3).AdjustToContents();

        using var ms = new MemoryStream();
        wb.SaveAs(ms);
        return ms.ToArray();
    }

    private static DateOnly? ParseMes(string? valor)
    {
        if (string.IsNullOrWhiteSpace(valor))
        {
            return null;
        }

        if (DateOnly.TryParseExact(
                valor,
                "yyyy-MM",
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var mes))
        {
            return new DateOnly(mes.Year, mes.Month, 1);
        }

        return null;
    }
}