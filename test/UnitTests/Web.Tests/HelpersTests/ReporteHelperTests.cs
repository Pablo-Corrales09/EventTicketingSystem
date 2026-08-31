using Web.Helpers;

namespace Web.Tests.HelpersTests;

public class ReporteHelperTests
{
    [Fact]
    public void ResolverRango_CuandoNoSeEnvianParametros_DevuelveElMesActualCompleto()
    {
        // Act
        var rango = ReporteHelper.ResolverRango(null, null);

        // Assert
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        rango.Desde.Should().Be(new DateOnly(hoy.Year, hoy.Month, 1));
        rango.Hasta.Should().Be(new DateOnly(hoy.Year, hoy.Month, DateTime.DaysInMonth(hoy.Year, hoy.Month)));
    }

    [Fact]
    public void ResolverRango_CuandoElHastaEsMenorQueElDesde_LosInvierte()
    {
        // Act
        var rango = ReporteHelper.ResolverRango("2026-08", "2026-03");

        // Assert
        rango.Desde.Should().Be(new DateOnly(2026, 3, 1));
        rango.Hasta.Should().Be(new DateOnly(2026, 8, 31));
    }

    [Fact]
    public void ResolverRango_CuandoLosMesesSonValidos_DevuelvePrimerYUltimoDia()
    {
        // Act
        var rango = ReporteHelper.ResolverRango("2026-02", "2026-02");

        // Assert
        rango.Desde.Should().Be(new DateOnly(2026, 2, 1));
        rango.Hasta.Should().Be(new DateOnly(2026, 2, 28));
    }

    [Fact]
    public void ResolverRango_CuandoElFormatoEsInvalido_UsaElMesActual()
    {
        // Act
        var rango = ReporteHelper.ResolverRango("no-es-un-mes", null);

        // Assert
        var hoy = DateOnly.FromDateTime(DateTime.Now);
        rango.Desde.Should().Be(new DateOnly(hoy.Year, hoy.Month, 1));
    }

    [Fact]
    public void FormatearPeriodo_CuandoEsElMismoMes_DevuelveUnSoloMes()
    {
        // Act
        var resultado = ReporteHelper.FormatearPeriodo(new DateOnly(2026, 8, 1), new DateOnly(2026, 8, 31));

        // Assert
        resultado.Should().Be(new DateOnly(2026, 8, 1).ToString("MMMM yyyy"));
    }

    [Fact]
    public void FormatearPeriodo_CuandoSonMesesDistintos_DevuelveElRango()
    {
        // Act
        var resultado = ReporteHelper.FormatearPeriodo(new DateOnly(2026, 3, 1), new DateOnly(2026, 8, 31));

        // Assert
        var esperado = $"{new DateOnly(2026, 3, 1).ToString("MMMM yyyy")} - {new DateOnly(2026, 8, 1).ToString("MMMM yyyy")}";
        resultado.Should().Be(esperado);
    }

    [Fact]
    public void GenerarExcel_CuandoHayResumenYEventos_DevuelveUnArchivoXlsxValido()
    {
        // Act
        var archivo = ReporteHelper.GenerarExcel(
            "Reporte de prueba",
            "agosto 2026",
            [("Total facturado", 500m), ("Boletos vendidos", 5)],
            [new ReporteEventoViewModel { NombreEvento = "Concierto", CantidadBoletos = 5, TotalFacturado = 500m }]);

        // Assert
        archivo.Should().NotBeEmpty();
        archivo[0].Should().Be(0x50); // 'P'
        archivo[1].Should().Be(0x4B); // 'K' (firma de archivos ZIP/xlsx)
    }
}