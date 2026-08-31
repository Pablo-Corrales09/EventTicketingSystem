namespace Web.Models;

public class UsuarioViewModel
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public int? IdRole { get; set; }

    public string? NombreRole { get; set; }

    public bool EsInactivo => NombreRole?.Equals("Inactivo", System.StringComparison.OrdinalIgnoreCase) == true;
}