using System.Net;
using Web.Controllers;

namespace Web.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_CuandoFaltanCampos_DevuelveBadRequest()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new AuthController(factory);

        // Act
        var resultado = await controller.Login(new LoginViewModel { Correo = "", Contrasena = "" });

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Login_CuandoLasCredencialesSonCorrectas_EstableceLasCookiesDeSesion()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new
        {
            token = "jwt-de-prueba",
            idUsuario = 1,
            nombre = "Ana",
            correo = "ana@correo.com",
            role = "admin"
        }));
        var controller = new AuthController(factory);
        var contexto = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = await controller.Login(new LoginViewModel { Correo = "ana@correo.com", Contrasena = "Clave123!" });

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("/api/Auth/login");

        var setCookie = contexto.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("AuthToken=jwt-de-prueba");
        setCookie.Should().Contain("AuthInfo=");
        setCookie.ToLowerInvariant().Should().Contain("httponly");
    }

    [Fact]
    public async Task Login_CuandoSeSolicitaRecordarme_AgregaExpiracionALasCookies()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new
        {
            token = "jwt-de-prueba",
            idUsuario = 1,
            nombre = "Ana",
            correo = "ana@correo.com",
            role = "cliente"
        }));
        var controller = new AuthController(factory);
        var contexto = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        await controller.Login(new LoginViewModel { Correo = "ana@correo.com", Contrasena = "Clave123!", Recordarme = true });

        // Assert
        var setCookie = contexto.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("expires=");
    }

    [Fact]
    public async Task Login_CuandoLasCredencialesSonIncorrectas_DevuelveUnauthorized()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ =>
            FakeHttpHandler.Json(new { mensaje = "Correo o contraseña incorrectos." }, HttpStatusCode.Unauthorized));
        var controller = new AuthController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Login(new LoginViewModel { Correo = "ana@correo.com", Contrasena = "MalaClave" });

        // Assert
        resultado.Should().BeOfType<UnauthorizedObjectResult>();
    }

    [Fact]
    public async Task Login_CuandoLaApiNoResponde_DevuelveStatusCode500()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new AuthController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Login(new LoginViewModel { Correo = "ana@correo.com", Contrasena = "Clave123!" });

        // Assert
        var statusCode = resultado.Should().BeOfType<ObjectResult>().Subject;
        statusCode.StatusCode.Should().Be(500);
    }

    [Fact]
    public async Task Registrar_CuandoLaContrasenaEsCorta_DevuelveBadRequest()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new AuthController(factory);

        // Act
        var resultado = await controller.Registrar(CrearRegistroValido(contrasena: "corta"));

        // Assert
        resultado.Should().BeOfType<BadRequestObjectResult>();
    }

    [Fact]
    public async Task Registrar_CuandoElRegistroFalla_DevuelveBadRequestConElMensajeDeLaApi()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ =>
            FakeHttpHandler.Json(new { error = "El correo ya está registrado." }, HttpStatusCode.BadRequest));
        var controller = new AuthController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Registrar(CrearRegistroValido());

        // Assert
        var badRequest = resultado.Should().BeOfType<BadRequestObjectResult>().Subject;
        var mensaje = (string)badRequest.Value!.GetType()!.GetProperty("mensaje")!.GetValue(badRequest.Value)!;
        mensaje.Should().Contain("El correo ya está registrado");
    }

    [Fact]
    public async Task Registrar_CuandoElRegistroEsExitoso_DevuelveOk()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { mensaje = "ok" }));
        var controller = new AuthController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Registrar(CrearRegistroValido());

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        handler.Solicitudes.Should().ContainSingle();
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("/api/Usuarios/crearUsuario");
    }

    [Fact]
    public void Estado_CuandoNoHayCookie_DevuelveNoAutenticado()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new AuthController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = controller.Estado();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var sesion = ok.Value.Should().BeOfType<UsuarioSesionViewModel>().Subject;
        sesion.Autenticado.Should().BeFalse();
    }

    [Fact]
    public void Estado_CuandoHayCookie_DevuelveLaSesionParseada()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new AuthController(factory);
        var contexto = new DefaultHttpContext();
        contexto.Request.Headers["Cookie"] = "AuthInfo=7|Ana|ana@correo.com|admin";
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = controller.Estado();

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var sesion = ok.Value.Should().BeOfType<UsuarioSesionViewModel>().Subject;
        sesion.Autenticado.Should().BeTrue();
        sesion.IdUsuario.Should().Be(7);
        sesion.Role.Should().Be("admin");
    }

    [Fact]
    public void Logout_EliminaLasCookiesDeSesion()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new AuthController(factory);
        var contexto = new DefaultHttpContext();
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };

        // Act
        var resultado = controller.Logout();

        // Assert
        resultado.Should().BeOfType<OkObjectResult>();
        var setCookie = contexto.Response.Headers["Set-Cookie"].ToString();
        setCookie.Should().Contain("AuthToken=");
        setCookie.Should().Contain("AuthInfo=");
        setCookie.Should().Contain("expires=");
    }

    private static UsuarioRegistroViewModel CrearRegistroValido(string contrasena = "Clave123!") =>
        new()
        {
            Nombre = "Ana",
            Apellido = "Pérez",
            Correo = "ana@correo.com",
            Telefono = "3001234567",
            Contrasena = contrasena
        };
}