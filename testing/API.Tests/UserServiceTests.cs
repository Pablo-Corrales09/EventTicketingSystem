using API.Data;
using API.Services;
using Microsoft.EntityFrameworkCore;

namespace API.Tests;

public class UserServiceTests
{
    private class TestDbContext : DbDevTicketappContext
    {
        protected override void OnConfiguring(
            DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.UseInMemoryDatabase(
                Guid.NewGuid().ToString()
            );
        }
    }

    [Fact]
    public async Task CrearUsuario_CorreoDuplicado_DebeLanzarExcepcion()
    {
        // Arrange
        using var context = new TestDbContext();

        var service = new UserService(context);

        var correo = "usuario.prueba@correo.com";

        await service.CrearUsuario(
            "Diego",
            "Prueba",
            null,
            correo,
            "Prueba123!",
            null
        );

        // Act
        var excepcion = await Assert.ThrowsAsync<InvalidOperationException>(
            async () =>
            {
                await service.CrearUsuario(
                    "Otro",
                    "Usuario",
                    null,
                    correo,
                    "OtraClave123!",
                    null
                );
            }
        );

        // Assert
        Assert.Equal(
            "El correo electronico ya esta registrado",
            excepcion.Message
        );
    }

    [Fact]
    public async Task CrearUsuario_DebeGuardarContrasenaEncriptada()
    {
        // Arrange
        using var context = new TestDbContext();

        var service = new UserService(context);

        var contrasena = "Prueba123!";

        // Act
        await service.CrearUsuario(
            "Diego",
            "Seguridad",
            null,
            "seguridad.prueba@correo.com",
            contrasena,
            null
        );

        var usuarioGuardado = await context.Usuarios.SingleAsync();

        // Assert
        Assert.NotEqual(
            contrasena,
            usuarioGuardado.PasswordHash
        );

        Assert.True(
            BCrypt.Net.BCrypt.Verify(
                contrasena,
                usuarioGuardado.PasswordHash
            )
        );
    }

    [Fact]
    public async Task BuscarPorCorreo_CorreoInexistente_DebeRetornarNull()
    {
        // Arrange
        using var context = new TestDbContext();

        var service = new UserService(context);

        // Act
        var resultado = await service.BuscarPorCorreoAsync(
            "noexiste@correo.com"
        );

        // Assert
        Assert.Null(resultado);
    }
}