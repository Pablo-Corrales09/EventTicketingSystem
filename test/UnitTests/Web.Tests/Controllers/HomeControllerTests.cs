using Microsoft.Extensions.Caching.Memory;
using Web.Controllers;

namespace Web.Tests.Controllers;

public class HomeControllerTests
{
    [Fact]
    public async Task Index_CuandoHayEventos_DevuelveVistaConLosEventosYElOrdenDelHero()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaEventos()));
        var controller = new HomeController(factory, new MemoryCache(new MemoryCacheOptions()));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.Model.Should().BeAssignableTo<List<EventoViewModel>>().Subject.Should().HaveCount(6);
        vista.ViewData["OrdenHero"].Should().BeAssignableTo<List<int>>().Subject.Should().HaveCount(5);
    }

    [Fact]
    public async Task Index_CuandoLaApiFalla_DevuelveVistaConError()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new HomeController(factory, new MemoryCache(new MemoryCacheOptions()));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Error"].Should().NotBeNull();
        vista.Model.Should().BeAssignableTo<List<EventoViewModel>>().Subject.Should().BeEmpty();
    }

    [Fact]
    public void Privacy_DevuelveVista()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new HomeController(factory, new MemoryCache(new MemoryCacheOptions()));
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = controller.Privacy();

        // Assert
        resultado.Should().BeOfType<ViewResult>();
    }

    private static List<EventoViewModel> ListaEventos() =>
        Enumerable.Range(1, 6)
            .Select(i => new EventoViewModel
            {
                IdEvento = i,
                NombreEvento = $"Evento {i}",
                FechaEvento = DateTime.Today.AddMonths(1),
                HoraEvento = new TimeOnly(20, 0)
            })
            .ToList();
}