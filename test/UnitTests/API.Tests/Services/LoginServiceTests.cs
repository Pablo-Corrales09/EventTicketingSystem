using API.Dtos;
using API.Services;

namespace API.Tests.Services;

public class LoginServiceTests
{
    [Fact]
    public async Task ValidarCredencialesAsync_CuandoLasCredencialesSonCorrectas_DevuelveElUsuario()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var usuario = DatosPrueba.CrearUsuario();
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        var service = new LoginService(contexto);

        // Act
        var resultado = await service.ValidarCredencialesAsync(
            new LoginRequestDto { Correo = usuario.Correo, Contrasena = DatosPrueba.ContrasenaValida });

        // Assert
        resultado.Should().NotBeNull();
        resultado!.IdUsuario.Should().Be(usuario.IdUsuario);
        resultado.IdRoleNavigation?.NombreRole.Should().Be("Cliente");
    }

    [Fact]
    public async Task ValidarCredencialesAsync_CuandoLaContrasenaEsIncorrecta_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var usuario = DatosPrueba.CrearUsuario();
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        var service = new LoginService(contexto);

        // Act
        var resultado = await service.ValidarCredencialesAsync(
            new LoginRequestDto { Correo = usuario.Correo, Contrasena = "ContrasenaErronea" });

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ValidarCredencialesAsync_CuandoElCorreoNoExiste_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var service = new LoginService(contexto);

        // Act
        var resultado = await service.ValidarCredencialesAsync(
            new LoginRequestDto { Correo = "noexiste@correo.com", Contrasena = DatosPrueba.ContrasenaValida });

        // Assert
        resultado.Should().BeNull();
    }

    [Fact]
    public async Task ValidarCredencialesAsync_CuandoElUsuarioEstaInactivo_DevuelveNull()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var usuario = DatosPrueba.CrearUsuario(role: DatosPrueba.CrearRole(nombre: "Inactivo"));
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();
        var service = new LoginService(contexto);

        // Act
        var resultado = await service.ValidarCredencialesAsync(
            new LoginRequestDto { Correo = usuario.Correo, Contrasena = DatosPrueba.ContrasenaValida });

        // Assert
        resultado.Should().BeNull();
    }
}