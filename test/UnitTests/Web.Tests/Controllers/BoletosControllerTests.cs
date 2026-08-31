using System.Net;
using System.Net.Http.Headers;
using Web.Controllers;

namespace Web.Tests.Controllers;

public class BoletosControllerTests
{
    private const string TipoExcel = "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet";

    [Fact]
    public async Task Index_CuandoEsAdmin_DevuelveVistaConReporte()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaBoletos()));
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Index(null, null, null, null, null);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["EsAdmin"].Should().Be(true);
        vista.ViewData["Reporte"].Should().NotBeNull();
        vista.Model.Should().BeAssignableTo<List<BoletoViewModel>>().Subject.Should().HaveCount(2);
    }

    [Fact]
    public async Task Index_CuandoEsClienteAutenticado_FiltraPorSuUsuario()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaBoletos()));
        var controller = new BoletosController(factory);
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers["Cookie"] = "AuthInfo=7|Ana|ana@correo.com|cliente";
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = await controller.Index(null, null, null, null, null);

        // Assert
        resultado.Should().BeOfType<ViewResult>();
        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].RequestUri!.Query.Should().Contain("usuarioId=7");
    }

    [Fact]
    public async Task Index_CuandoNoHaySesion_DevuelveVistaConAvisoSinLlamarALaApi()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaBoletos()));
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Index(null, null, null, null, null);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Aviso"].Should().NotBeNull();
        vista.Model.Should().BeAssignableTo<List<BoletoViewModel>>().Subject.Should().BeEmpty();
        handler.Solicitudes.Should().BeEmpty();
    }

    [Fact]
    public async Task Index_CuandoLaApiFalla_DevuelveVistaConError()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Index(null, null, null, null, null);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Error"].Should().NotBeNull();
    }

    [Fact]
    public async Task Details_CuandoElBoletoNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => RespuestaNull());
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Details(99);

        // Assert
        resultado.Should().BeOfType<NotFoundResult>();
    }

    [Fact]
    public async Task Details_CuandoElBoletoExiste_DevuelveVistaConElBoleto()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(
            new BoletoViewModel { IdBoleto = 1, NumBoleto = "BOL-CO-1-1", NombreEvento = "Concierto" }));
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Details(1);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.Model.Should().BeOfType<BoletoViewModel>().Subject.NumBoleto.Should().Be("BOL-CO-1-1");
    }

    [Fact]
    public async Task ExportarExcel_CuandoNoEsAdmin_RedirigeAlIndex()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaBoletos()));
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.ExportarExcel(null, null, null, null, null);

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(BoletosController.Index));
    }

    [Fact]
    public async Task ExportarExcel_CuandoEsAdmin_DevuelveArchivoXlsx()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaBoletos()));
        var controller = new BoletosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.ExportarExcel(null, null, null, null, null);

        // Assert
        var archivo = resultado.Should().BeOfType<FileContentResult>().Subject;
        archivo.ContentType.Should().Be(TipoExcel);
        archivo.FileContents.Should().NotBeEmpty();
        archivo.FileContents[0].Should().Be(0x50); // 'P' (firma xlsx)
        archivo.FileContents[1].Should().Be(0x4B); // 'K'
        archivo.FileDownloadName.Should().Contain("Reporte_Boletos_");
    }

    private static List<BoletoViewModel> ListaBoletos() =>
    [
        new()
        {
            IdBoleto = 1,
            NumBoleto = "BOL-CO-1-1",
            FechaCompra = DateOnly.FromDateTime(DateTime.Now),
            Precio = 100m,
            NombreEvento = "Concierto de Rock",
            NombreUsuario = "Ana",
            ApellidoUsuario = "Pérez"
        },
        new()
        {
            IdBoleto = 2,
            NumBoleto = "BOL-CO-1-2",
            FechaCompra = DateOnly.FromDateTime(DateTime.Now),
            Precio = 100m,
            NombreEvento = "Concierto de Rock",
            NombreUsuario = "Luis",
            ApellidoUsuario = "Gómez"
        }
    ];

    private static HttpResponseMessage RespuestaNull() =>
        new(HttpStatusCode.OK)
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