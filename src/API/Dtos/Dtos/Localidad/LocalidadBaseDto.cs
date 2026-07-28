using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public abstract class LocalidadBaseDto
    {
    [Required(ErrorMessage ="El nombre de la localidad es obligatorio")]
    [StringLength(100, ErrorMessage = "El nombre de la localidad no puede exceder los 100 caracteres.")]
    public string NombreLocalidad { get; set; } = null!;
    
    [Range(1, int.MaxValue, ErrorMessage = "Debe seleccionar una sede válida para la localidad.")]
    public int IdSedeEvento {get; set;}
    }
}         