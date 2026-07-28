using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class UsuarioCreacionDto : UsuarioBaseDto
    {
        [Required(ErrorMessage ="La contraseña es obligatoria.")]
        [StringLength(50, MinimumLength =8, ErrorMessage = "La contraseña debe tener entre 8 y 50 caracteres.")]
        public string Contrasena { get; set; } = null!; 
    }
}