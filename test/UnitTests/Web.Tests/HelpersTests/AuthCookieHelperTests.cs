using Web.Helpers;

namespace Web.Tests.HelpersTests;

public class AuthCookieHelperTests
{
    [Fact]
    public void ObtenerSesion_CuandoNoHayCookie_DevuelveNoAutenticado()
    {
        // Act
        var sesion = AuthCookieHelper.ObtenerSesion(CrearRequest());

        // Assert
        sesion.Autenticado.Should().BeFalse();
    }

    [Fact]
    public void ObtenerSesion_CuandoLaCookieEsValida_ParseaTodosLosCampos()
    {
        // Act
        var sesion = AuthCookieHelper.ObtenerSesion(CrearRequest("AuthInfo", "7|Ana|ana@correo.com|admin"));

        // Assert
        sesion.Autenticado.Should().BeTrue();
        sesion.IdUsuario.Should().Be(7);
        sesion.Nombre.Should().Be("Ana");
        sesion.Correo.Should().Be("ana@correo.com");
        sesion.Role.Should().Be("admin");
    }

    [Fact]
    public void ObtenerSesion_CuandoElIdNoEsNumerico_DevuelveIdCero()
    {
        // Act
        var sesion = AuthCookieHelper.ObtenerSesion(CrearRequest("AuthInfo", "no-numerico|Ana|ana@correo.com|cliente"));

        // Assert
        sesion.Autenticado.Should().BeTrue();
        sesion.IdUsuario.Should().Be(0);
    }

    [Fact]
    public void EsAdmin_CuandoElRolContieneAdminEnCualquierCaso_DevuelveTrue()
    {
        // Act
        var resultado = AuthCookieHelper.EsAdmin(CrearRequest("AuthInfo", "1|Ana|ana@correo.com|Administrador"));

        // Assert
        resultado.Should().BeTrue();
    }

    [Fact]
    public void EsAdmin_CuandoElRolEsCliente_DevuelveFalse()
    {
        // Act
        var resultado = AuthCookieHelper.EsAdmin(CrearRequest("AuthInfo", "1|Ana|ana@correo.com|Cliente"));

        // Assert
        resultado.Should().BeFalse();
    }

    [Fact]
    public void EsAdmin_CuandoNoHayCookie_DevuelveFalse()
    {
        // Act
        var resultado = AuthCookieHelper.EsAdmin(CrearRequest());

        // Assert
        resultado.Should().BeFalse();
    }

    private static HttpRequest CrearRequest(string nombreCookie = "", string valorCookie = "")
    {
        var contexto = new DefaultHttpContext();
        if (!string.IsNullOrEmpty(nombreCookie))
        {
            contexto.Request.Headers["Cookie"] = $"{nombreCookie}={valorCookie}";
        }
        return contexto.Request;
    }
}