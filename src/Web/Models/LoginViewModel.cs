using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class LoginViewModel
{
    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "Ingrese un correo válido.")]
    public string Correo { get; set; } = string.Empty;

    [Required(ErrorMessage = "La contraseña es obligatoria.")]
    [DataType(DataType.Password)]
    public string Contrasena { get; set; } = string.Empty;
}

public class LoginResponseViewModel
{
    public string Token { get; set; } = string.Empty;

    public string Mensaje { get; set; } = string.Empty;
}

public class UsuarioSesionViewModel
{
    public int IdUsuario { get; set; }

    public string Nombre { get; set; } = string.Empty;

    public string Apellido { get; set; } = string.Empty;

    public string Correo { get; set; } = string.Empty;

    public string? Telefono { get; set; }

    public int? IdRole { get; set; }

    public string? NombreRole { get; set; }
}