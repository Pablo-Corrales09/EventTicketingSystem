namespace Web.Models;

public class UsuarioSesionViewModel
{
    public bool Autenticado { get; set; }

    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string? Role { get; set; }
}
