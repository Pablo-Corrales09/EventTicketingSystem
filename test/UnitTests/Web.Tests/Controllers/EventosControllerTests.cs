using Web.Controllers;

namespace Web.Tests.Controllers;

public class EventosControllerTests
{
    [Fact]
    public async Task Index_CuandoEsAdmin_ConsumeLaListaDeGestion()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaEventosSede()));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["EsAdmin"].Should().Be(true);
        vista.ViewData["Titulo"].Should().Be("Gestión de Eventos");
        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Eventos/localidad-sede");
    }

    [Fact]
    public async Task Index_CuandoEsClienteAutenticado_ConsumeSusEventos()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaEventosSede()));
        var controller = new EventosController(factory);
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers["Cookie"] = "AuthInfo=7|Ana|ana@correo.com|cliente";
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Titulo"].Should().Be("Mis Eventos");
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Eventos/mis-eventos");
        handler.Solicitudes[0].RequestUri!.Query.Should().Contain("usuarioId=7");
    }

    [Fact]
    public async Task Index_CuandoNoHaySesion_ConsumeLosEventosDisponibles()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(ListaEventosSede()));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Titulo"].Should().Be("Eventos Disponibles");
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Eventos/localidad-sede");
    }

    [Fact]
    public async Task Index_CuandoLaApiFalla_DevuelveVistaConError()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Error"].Should().NotBeNull();
    }

    [Fact]
    public async Task ObtenerDetalles_CuandoElEventoExiste_DevuelveJsonConLocalidades()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(request =>
            request.RequestUri!.AbsolutePath.EndsWith("api/Eventos/5")
                ? FakeHttpHandler.Json(ListaEventosSede()[0])
                : FakeHttpHandler.Json(new List<EventoLocalidadViewModel>
                {
                    new()
                    {
                        IdEventoLocalidad = 1,
                        IdEvento = 5,
                        NombreLocalidad = "VIP",
                        Precio = 100m,
                        CapacidadDisponible = 50
                    }
                }));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.ObtenerDetalles(5);

        // Assert
        var json = resultado.Should().BeOfType<JsonResult>().Subject;
        var evento = json.Value.Should().BeOfType<EventoSedeViewModel>().Subject;
        evento.IdEvento.Should().Be(5);
        evento.Localidades.Should().ContainSingle();
    }

    [Fact]
    public async Task ObtenerDetalles_CuandoElEventoNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => RespuestaNull());
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.ObtenerDetalles(99);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Details_CuandoElEventoNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => RespuestaNull());
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Details(99);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Crear_CuandoNoEsAdmin_RedirigeAlIndex()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Crear();

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(EventosController.Index));
    }

    [Fact]
    public async Task Crear_CuandoEsAdminYElModeloEsInvalido_DevuelveVistaYCargaLasSedes()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(
            new List<SedeViewModel>
            {
                new() { IdSedeEvento = 1, NombreSedeEvento = "Coliseo Central", Ubicacion = "Av. Principal 123" }
            }));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };
        controller.ModelState.AddModelError("NombreEvento", "El nombre es obligatorio.");

        // Act
        var resultado = await controller.Crear(new EventoCreacionViewModel());

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Sedes");
        vista.Model.Should().BeOfType<EventoCreacionViewModel>().Subject.Sedes.Should().ContainSingle();
    }

    [Fact]
    public async Task Crear_CuandoEsAdminYLaApiRespondeOk_RedirigeAlIndex()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { mensaje = "ok" }));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Crear(CrearModeloEvento());

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(EventosController.Index));
        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Eventos/crear");
        handler.Solicitudes[0].Content.Should().BeOfType<MultipartFormDataContent>();
    }

    [Fact]
    public async Task Editar_CuandoNoEsAdmin_RedirigeAlIndex()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Editar(5);

        // Assert
        resultado.Should().BeOfType<RedirectToActionResult>();
    }

    [Fact]
    public async Task Editar_CuandoElEventoNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => RespuestaNull());
        var controller = new EventosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Editar(99);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    private static List<EventoSedeViewModel> ListaEventosSede() =>
    [
        new()
        {
            IdEvento = 5,
            NombreEvento = "Concierto de Rock",
            FechaEvento = DateTime.Today.AddMonths(1),
            HoraEvento = new TimeSpan(20, 0, 0),
            Sede = new SedeViewModel { IdSedeEvento = 1, NombreSedeEvento = "Coliseo Central", Ubicacion = "Av. Principal 123" }
        }
    ];

    private static EventoCreacionViewModel CrearModeloEvento() =>
        new()
        {
            NombreEvento = "Concierto de Rock",
            FechaEvento = DateTime.Today.AddMonths(1),
            HoraEvento = new TimeOnly(20, 0),
            IdSede = 1
        };

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