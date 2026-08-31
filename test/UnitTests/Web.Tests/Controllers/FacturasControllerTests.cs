using Web.Controllers;

namespace Web.Tests.Controllers;

public class FacturasControllerTests
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Fact]
    public async Task Index_CuandoEsAdmin_DevuelveVistaConReporte()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaFacturas()));
        var controller = new FacturasController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Index(null, null, null, null, null, null);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["EsAdmin"].Should().Be(true);
        vista.ViewData["Reporte"].Should().NotBeNull();
        vista.ViewData["RangoReporte"].Should().NotBeNull();
        vista.Model.Should().BeAssignableTo<List<FacturaViewModel>>().Subject.Should().HaveCount(1);
    }

    [Fact]
    public async Task Index_CuandoNoHaySesion_DevuelveVistaConAvisoSinLlamarALaApi()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaFacturas()));
        var controller = new FacturasController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Index(null, null, null, null, null, null);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Aviso"].Should().NotBeNull();
        handler.Solicitudes.Should().BeEmpty();
    }

    [Fact]
    public async Task Index_CuandoEsClienteAutenticado_FiltraPorSuUsuario()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaFacturas()));
        var controller = new FacturasController(factory);
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers["Cookie"] = "AuthInfo=7|Ana|ana@correo.com|cliente";
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = await controller.Index(null, null, null, null, null, null);

        // Assert
        resultado.Should().BeOfType<ViewResult>();
        handler.Solicitudes[0].RequestUri!.Query.Should().Contain("usuarioId=7");
    }

    [Fact]
    public async Task Imprimir_CuandoLaFacturaNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => RespuestaNull());
        var controller = new FacturasController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Imprimir(99);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Imprimir_CuandoLaFacturaExiste_DevuelveVistaConLaFactura()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(
            new FacturaViewModel { IdFactura = 1, NumeroFactura = "FAC-2026-000001", Total = 100m }));
        var controller = new FacturasController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Imprimir(1);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.Model.Should().BeOfType<FacturaViewModel>().Subject.NumeroFactura.Should().Be("FAC-2026-000001");
    }

    [Fact]
    public async Task ExportarExcel_CuandoNoEsAdmin_RedirigeAlIndex()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaFacturas()));
        var controller = new FacturasController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.ExportarExcel(null, null, null, null, null, null);

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(FacturasController.Index));
    }

    [Fact]
    public async Task ExportarExcel_CuandoEsAdmin_DevuelveArchivoXlsx()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaFacturas()));
        var controller = new FacturasController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.ExportarExcel(null, null, null, null, null, null);

        // Assert
        var archivo = resultado.Should().BeOfType<FileContentResult>().Subject;
        archivo.ContentType.Should().Be(TipoExcel);
        archivo.FileContents.Should().NotBeEmpty();
        archivo.FileDownloadName.Should().Contain("Reporte_Facturacion_");
    }

    private static List<FacturaViewModel> ListaFacturas() =>
    [
        new()
        {
            IdFactura = 1,
            IdUsuario = 1,
            IdMedioPago = 1,
            NumeroFactura = "FAC-2026-000001",
            Total = 200m,
            Boletos =
            [
                new BoletoViewModel
                {
                    IdBoleto = 1,
                    NumBoleto = "BOL-CO-1-1",
                    FechaCompra = DateOnly.FromDateTime(DateTime.Now),
                    Precio = 100m,
                    NombreEvento = "Concierto de Rock",
                    NombreUsuario = "Ana",
                    ApellidoUsuario = "Pérez"
                },
                new BoletoViewModel
                {
                    IdBoleto = 2,
                    NumBoleto = "BOL-CO-1-2",
                    FechaCompra = DateOnly.FromDateTime(DateTime.Now),
                    Precio = 100m,
                    NombreEvento = "Concierto de Rock",
                    NombreUsuario = "Luis",
                    ApellidoUsuario = "Gómez"
                }
            ]
        }
    ];

    private static HttpResponseMessage RespuestaNull() =>
        new(System.Net.HttpStatusCode.OK)
        {
            Content = new StringContent("null", System.Text.Encoding.UTF8, "application/json")
        };

    private static DefaultHttpContext CrearContextoAdmin()
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers["Cookie"] = "AuthInfo=1|Admin|admin@correo.com|admin";
        return contexto;
    }
}