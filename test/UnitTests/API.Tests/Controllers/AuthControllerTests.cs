using System.IdentityModel.Tokens.Jwt;
using API.Controllers;
using API.Dtos;
using API.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Controllers;

public class AuthControllerTests
{
    [Fact]
    public async Task Login_CuandoLasCredencialesSonCorrectas_DevuelveOkConToken()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var usuario = DatosPrueba.CrearUsuario();
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var controller = new AuthController(
            new LoginService(contexto),
            new TokenService(CrearConfiguracionValida()));

        // Act
        var resultado = await controller.Login(
            new LoginRequestDto { Correo = usuario.Correo, Contrasena = DatosPrueba.ContrasenaValida });

        // Assert
        var ok = resultado.Should().BeOfType<OkObjectResult>().Subject;
        var token = (string)ok.Value!.GetType()!.GetProperty("token")!.GetValue(ok.Value)!;
        token.Should().NotBeNullOrWhiteSpace();
        new JwtSecurityTokenHandler().ReadJwtToken(token).Should().NotBeNull();
    }

    [Fact]
    public async Task Login_CuandoLasCredencialesSonIncorrectas_DevuelveUnauthorized()
    {
        // Arrange
        using var contexto = await ContextoPrueba.CrearAsync();
        var usuario = DatosPrueba.CrearUsuario();
        contexto.Usuarios.Add(usuario);
        await contexto.SaveChangesAsync();

        var controller = new AuthController(
            new LoginService(contexto),
            new TokenService(CrearConfiguracionValida()));

        // Act
        var resultado = await controller.Login(
            new LoginRequestDto { Correo = usuario.Correo, Contrasena = "ContrasenaErronea" });

        // Assert
        resultado.Should().BeOfType<UnauthorizedObjectResult>();
    }

    private static IConfiguration CrearConfiguracionValida()
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.SetupGet(s => s["Secret"]).Returns("clave-secreta-de-prueba-para-firmar-tokens-12345");
        jwtSection.SetupGet(s => s["ExpireDays"]).Returns("1");
        jwtSection.SetupGet(s => s["Issuer"]).Returns("API.Tests");
        jwtSection.SetupGet(s => s["Audience"]).Returns("API.Tests");
        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        return config.Object;
    }
}