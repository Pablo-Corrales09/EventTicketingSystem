using Microsoft.AspNetCore.Mvc.ViewFeatures;
using Web.Controllers;

namespace Web.Tests.Controllers;

public class UsuariosControllerTests
{
    [Fact]
    public async Task Index_CuandoNoEsAdmin_RedirigeAlHome()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { }));
        var controller = new UsuariosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = new DefaultHttpContext() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be("Index");
        redirect.ControllerName.Should().Be("Home");
    }

    [Fact]
    public async Task Index_CuandoEsAdmin_DevuelveVistaConLosUsuarios()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(
            new List<UsuarioViewModel>
            {
                new()
                {
                    IdUsuario = 1,
                    Nombre = "Ana",
                    Apellido = "Pérez",
                    Correo = "ana@correo.com",
                    NombreRole = "admin"
                },
                new()
                {
                    IdUsuario = 2,
                    Nombre = "Luis",
                    Apellido = "Gómez",
                    Correo = "luis@correo.com",
                    NombreRole = "cliente"
                }
            }));
        var controller = new UsuariosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Index();

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        vista.ViewData["EsAdmin"].Should().Be(true);
        vista.Model.Should().BeAssignableTo<List<UsuarioViewModel>>().Subject.Should().HaveCount(2);
    }

    [Fact]
    public async Task Editar_CuandoEsAdminYElUsuarioExiste_DevuelveVistaConRoles()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(request =>
            request.RequestUri!.AbsolutePath.EndsWith("api/Usuarios/roles")
                ? FakeHttpHandler.Json(new List<RoleViewModel>
                {
                    new() { IdRole = 1, NombreRole = "admin" },
                    new() { IdRole = 2, NombreRole = "cliente" }
                })
                : FakeHttpHandler.Json(new UsuarioViewModel
                {
                    IdUsuario = 5,
                    Nombre = "Ana",
                    Apellido = "Pérez",
                    Correo = "ana@correo.com",
                    IdRole = 2
                }));
        var controller = new UsuariosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Editar(5);

        // Assert
        var vista = resultado.Should().BeOfType<ViewResult>().Subject;
        var modelo = vista.Model.Should().BeOfType<UsuarioEdicionViewModel>().Subject;
        modelo.IdUsuario.Should().Be(5);
        modelo.Roles.Should().HaveCount(2);
    }

    [Fact]
    public async Task Editar_CuandoElUsuarioNoExiste_DevuelveNotFound()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => RespuestaNull());
        var controller = new UsuariosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Editar(99);

        // Assert
        resultado.Should().BeOfType<NotFoundObjectResult>();
    }

    [Fact]
    public async Task Editar_CuandoLaApiRespondeOk_RedirigeAlIndex()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { mensaje = "ok" }));
        var controller = new UsuariosController(factory);
        controller.ControllerContext = new ControllerContext { HttpContext = CrearContextoAdmin() };

        // Act
        var resultado = await controller.Editar(5, new UsuarioEdicionViewModel
        {
            IdUsuario = 5,
            Nombre = "Ana",
            Apellido = "Pérez",
            Correo = "ana@correo.com",
            IdRole = 2
        });

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsuariosController.Index));
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Usuarios/actualizar/5");
    }

    [Fact]
    public async Task Desactivar_CuandoLaApiRespondeOk_RedirigeConMensajeDeExito()
    {
        // Arrange
        var (factory, handler) = FakeHttpClientFactory.Crear(_ => FakeHttpHandler.Json(new { mensaje = "ok" }));
        var controller = new UsuariosController(factory);
        var contexto = CrearContextoAdmin();
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };
        AsignarTempData(controller, contexto);

        // Act
        var resultado = await controller.Desactivar(5);

        // Assert
        var redirect = resultado.Should().BeOfType<RedirectToActionResult>().Subject;
        redirect.ActionName.Should().Be(nameof(UsuariosController.Index));
        controller.TempData["Exito"].Should().NotBeNull();
        handler.Solicitudes[0].RequestUri!.AbsolutePath.Should().EndWith("api/Usuarios/desactivar/5");
    }

    [Fact]
    public async Task Activar_CuandoLaApiFalla_RedirigeConMensajeDeError()
    {
        // Arrange
        var (factory, _) = FakeHttpClientFactory.Crear(_ => throw new HttpRequestException("sin conexión"));
        var controller = new UsuariosController(factory);
        var contexto = CrearContextoAdmin();
        controller.ControllerContext = new ControllerContext { HttpContext = contexto };
        AsignarTempData(controller, contexto);

        // Act
        var resultado = await controller.Activar(5);

        // Assert
        resultado.Should().BeOfType<RedirectToActionResult>();
        controller.TempData["Error"].Should().NotBeNull();
    }

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

    private static void AsignarTempData(Controller controller, DefaultHttpContext contexto)
    {
        var provider = new Mock<ITempDataProvider>();
        provider.Setup(p => p.LoadTempData(It.IsAny<HttpContext>())).Returns(new Dictionary<string, object>());
        controller.TempData = new TempDataDictionary(contexto, provider.Object);
    }
}