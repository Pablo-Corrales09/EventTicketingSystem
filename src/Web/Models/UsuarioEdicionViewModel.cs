using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class UsuarioEdicionViewModel
{
    public int IdUsuario { get; set; }

    [Required(ErrorMessage = "El nombre es obligatorio.")]
    [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
    public string Nombre { get; set; } = string.Empty;

    [Required(ErrorMessage = "El apellido es obligatorio.")]
    [StringLength(50, ErrorMessage = "El apellido no puede exceder los 50 caracteres.")]
    public string Apellido { get; set; } = string.Empty;

    [Required(ErrorMessage = "El correo es obligatorio.")]
    [EmailAddress(ErrorMessage = "El formato del correo no es válido.")]
    [StringLength(100, ErrorMessage = "El correo no puede exceder los 100 caracteres.")]
    public string Correo { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "El teléfono no puede exceder los 20 caracteres.")]
    public string? Telefono { get; set; }

    [Range(1, int.MaxValue, ErrorMessage = "Debes seleccionar un rol.")]
    public int? IdRole { get; set; }

    public List<RoleViewModel> Roles { get; set; } = new();
}