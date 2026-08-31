using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using API.Services;
using Microsoft.Extensions.Configuration;

namespace API.Tests.Services;

public class TokenServiceTests
{
    private const string ClaveSecreta = "clave-secreta-de-prueba-para-firmar-tokens-12345";

    [Fact]
    public void GenerarToken_CuandoLaConfiguracionEsValida_DevuelveJwtConElRolDelUsuario()
    {
        // Arrange
        var service = new TokenService(CrearConfiguracionMock());
        var usuario = DatosPrueba.CrearUsuario(id: 7, role: DatosPrueba.CrearRole(id: 2, nombre: "admin"));

        // Act
        var token = service.GenerarToken(usuario);

        // Assert
        token.Should().NotBeNullOrWhiteSpace();
        var jwt = new JwtSecurityTokenHandler().ReadJwtToken(token);
        jwt.Claims.Should().Contain(c => c.Type == "nameid" && c.Value == "7");
        jwt.Claims.Should().Contain(c => c.Type == "role" && c.Value == "admin");
        jwt.Claims.Should().Contain(c => c.Type == "email" && c.Value == usuario.Correo);
    }

    [Fact]
    public void GenerarToken_CuandoFaltaLaClaveSecreta_LanzaInvalidOperationException()
    {
        // Arrange
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.SetupGet(s => s["Secret"]).Returns((string?)null);
        jwtSection.SetupGet(s => s["ExpireDays"]).Returns("1");
        jwtSection.SetupGet(s => s["Issuer"]).Returns("API.Tests");
        jwtSection.SetupGet(s => s["Audience"]).Returns("API.Tests");
        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        var service = new TokenService(config.Object);

        // Act
        var accion = () => service.GenerarToken(DatosPrueba.CrearUsuario());

        // Assert
        accion.Should().Throw<InvalidOperationException>()
            .WithMessage("*JwtSettings:Secret*");
    }

    private static IConfiguration CrearConfiguracionMock()
    {
        var jwtSection = new Mock<IConfigurationSection>();
        jwtSection.SetupGet(s => s["Secret"]).Returns(ClaveSecreta);
        jwtSection.SetupGet(s => s["ExpireDays"]).Returns("1");
        jwtSection.SetupGet(s => s["Issuer"]).Returns("API.Tests");
        jwtSection.SetupGet(s => s["Audience"]).Returns("API.Tests");
        var config = new Mock<IConfiguration>();
        config.Setup(c => c.GetSection("JwtSettings")).Returns(jwtSection.Object);
        return config.Object;
    }
}