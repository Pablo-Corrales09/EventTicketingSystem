using Web.Models;

namespace Web.Helpers;

public static class AuthCookieHelper
{
    public const string TokenCookie = "AuthToken";
    public const string InfoCookie = "AuthInfo";

    public static UsuarioSesionViewModel ObtenerSesion(HttpRequest request)
    {
        var info = request.Cookies[InfoCookie];

        if (string.IsNullOrWhiteSpace(info))
        {
            return new UsuarioSesionViewModel { Autenticado = false };
        }

        var partes = info.Split('|');

        return new UsuarioSesionViewModel
        {
            Autenticado = true,
            IdUsuario = int.TryParse(partes.Length > 0 ? partes[0] : null, out var id) ? id : 0,
            Nombre = partes.Length > 1 ? partes[1] : string.Empty,
            Correo = partes.Length > 2 ? partes[2] : string.Empty,
            Role = partes.Length > 3 ? partes[3] : null
        };
    }

    public static bool EsAdmin(HttpRequest request)
    {
        var rol = ObtenerSesion(request).Role;
        return !string.IsNullOrWhiteSpace(rol) &&
               rol.Contains("admin", StringComparison.OrdinalIgnoreCase);
    }
}
