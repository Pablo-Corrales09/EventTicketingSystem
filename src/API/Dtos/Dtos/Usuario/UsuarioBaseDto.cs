using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public abstract class UsuarioBaseDto
    {
        [Required(ErrorMessage ="El nombre es obligatorio")]
        [StringLength(50, ErrorMessage ="El nombre puede contener 50 caracteres como maximo.")]
        public string Nombre { get; set; } = null!;
        
        [Required(ErrorMessage ="El apellido es obligatorio")]
        [StringLength(50, ErrorMessage ="El apellido puede contener 50 caracteres como maximo.")]
        public string Apellido { get; set; } = null!;
        
        [Required(ErrorMessage = "El correo es obligatorio")]
        [EmailAddress(ErrorMessage ="El formato de correo electronico no es valido.")]
        [StringLength(100, ErrorMessage ="El correo puede contener 100 caracteres como maximo")]
        public string Correo { get; set; } = null!;
        public string? Telefono { get; set; }
        

        [Range(1, int.MaxValue, ErrorMessage ="Debe seleccionar un rol valido para crear el usuario.")]
        public int? IdRole { get; set; }
    }
}