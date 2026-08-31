using System.Net;
using Web.Controllers;

namespace Web.Tests.Controllers;

public class CompraControllerTests
{
    [Fact]
    public async Task Comprar_CuandoLaLocalidadNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(request =>
            request.RequestUri!.AbsolutePath.EndsWith("api/EventosLocalidades")
                ? FakeHttpHandler.Json(new List<EventoLocalidadViewModel>())
                : FakeHttpHandler.Json(new List<MedioPagoViewModel>()));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Comprar(99);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Comprar_CuandoLaLocalidadExiste_DevuelveVistaConElModeloDePago()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(request =>
            request.RequestUri!.AbsolutePath.EndsWith("api/EventosLocalidades")
                ? FakeHttpHandler.Json(new List<EventoLocalidadViewModel>
                {
                    new()
                    {
                        IdEventoLocalidad = 5,
                        IdEvento = 1,
                        NombreEvento = "Concierto de Rock",
                        NombreLocalidad = "VIP",
                        Precio = 100m,
                        CapacidadDisponible = 50
                    }
                })
                : FakeHttpHandler.Json(new List<MedioPagoViewModel>
                {
                    new() { IdMedioPago = 1, Nombre = "Tarjeta de crédito" }
                }));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Comprar(5, cantidad: 3);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        var modelo = vista.Model.Should().BeOfType<ProcesoPagoViewModel>().Subject;
        modelo.IdEventoLocalidad.Should().Be(5);
        modelo.NombreEvento.Should().Be("Concierto de Rock");
        modelo.Cantidad.Should().Be(3);
        vista.ViewData["MediosPago"].Should().BeAssignableTo<List<MedioPagoViewModel>>().Subject.Should().ContainSingle();
    }

    [Fact]
    public async Task Comprar_CuandoLaCantidadExcedeLaCapacidad_LaAjustaAlMaximo()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(request =>
            request.RequestUri!.AbsolutePath.EndsWith("api/EventosLocalidades")
                ? FakeHttpHandler.Json(new List<EventoLocalidadViewModel>
                {
                    new()
                    {
                        IdEventoLocalidad = 5,
                        IdEvento = 1,
                        NombreEvento = "Concierto",
                        NombreLocalidad = "VIP",
                        Precio = 100m,
                        CapacidadDisponible = 10
                    }
                })
                : FakeHttpHandler.Json(new List<MedioPagoViewModel>()));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Comprar(5, cantidad: 999);

        // Assert
        var modelo = resultado.Should().BeOfType<ViewResult>().Subject.Model.Should().BeOfType<ProcesoPagoViewModel>().Subject;
        modelo.Cantidad.Should().Be(10);
    }

    [Fact]
    public async Task Comprar_CuandoLaApiNoResponde_DevuelveVistaConError()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Comprar(5);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["Error"].Should().NotBeNull();
    }

    [Fact]
    public async Task ProcesarPago_CuandoNoHaySesion_DevuelveRequiereLogin()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago());

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var requiereLogin = (bool)ok.Value!.GetType()!.GetProperty("requiereLogin")!.GetValue(ok.Value)!;
        requiereLogin.Should().BeTrue();
    }

    [Fact]
    public async Task ProcesarPago_CuandoElMedioDePagoEsInvalido_DevuelveBadRequest()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new CompraController(factory);
        var contexto = CrearContextoAutenticado();
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago(idMedioPago: 99));

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ProcesarPago_CuandoHaySesion_EnviaElTokenBearerYDevuelveExito()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ =>
            FakeHttpHandler.Json(new BoletoViewModel { IdBoleto = 42, NumBoleto = "BOL-CO-1-1" }));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAutenticado() };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago());

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var exito = (bool)ok.Value!.GetType()!.GetProperty("exito")!.GetValue(ok.Value)!;
        exito.Should().BeTrue();
        var idBoleto = (int)ok.Value!.GetType()!.GetProperty("idBoleto")!.GetValue(ok.Value)!;
        idBoleto.Should().Be(42);

        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].Headers.Authorization.Should().NotBeNull();
        handler.Solicitudes[0].Headers.Authorization!.Scheme.Should().Be("Bearer");
        handler.Solicitudes[0].Headers.Authorization.Parameter.Should().Be("jwt-de-prueba");
    }

    [Fact]
    public async Task ProcesarPago_CuandoLaApiDevuelveUnauthorized_DevuelveRequiereLogin()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ =>
            FakeHttpHandler.Json(new { mensaje = "no autorizado" }, HttpStatusCode.Unauthorized));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAutenticado() };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago());

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var requiereLogin = (bool)ok.Value!.GetType()!.GetProperty("requiereLogin")!.GetValue(ok.Value)!;
        requiereLogin.Should().BeTrue();
    }

    [Fact]
    public async Task ProcesarPago_CuandoLaApiDevuelveTooManyRequests_DevuelveStatusCode429()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ =>
            FakeHttpHandler.Json(new { mensaje = "límite superado" }, HttpStatusCode.TooManyRequests));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAutenticado() };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago());

        // Assert
        var objectResult = resultado.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(429);
    }

    [Fact]
    public async Task ProcesarPago_CuandoLaApiDevuelveErrorDeNegocio_DevuelveBadRequest()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ =>
            FakeHttpHandler.Json(new { mensaje = "No hay suficientes boletos disponibles." }, HttpStatusCode.BadRequest));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAutenticado() };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago());

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task ProcesarPago_CuandoLaApiNoResponde_DevuelveStatusCode500()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new CompraController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAutenticado() };

        // Act
        var resultado = await controller.ProcesarPago(CrearModeloPago());

        // Assert
        var objectResult = resultado.Should().BeOfType<ObjectResult>().Subject;
        objectResult.StatusCode.Should().Be(500);
    }

    private static ProcesoPagoViewModel CrearModeloPago(int idMedioPago = 1) =>
        new()
        {
            IdEvento = 1,
            IdEventoLocalidad = 5,
            NombreEvento = "Concierto de Rock",
            NombreLocalidad = "VIP",
            Precio = 100m,
            Cantidad = 2,
            IdMedioPago = idMedioPago
        };

    private static DefaultHttpContext CrearContextoAutenticado()
    {
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers["Cookie"] =
            "AuthInfo=7|Ana|ana@correo.com|cliente; AuthToken=jwt-de-prueba";
        return contexto;
    }
}