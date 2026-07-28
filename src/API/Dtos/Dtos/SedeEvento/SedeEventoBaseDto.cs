using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public abstract class SedeEventoBaseDto
    {
        [Required (ErrorMessage ="El nombre de la sede es obligatorio")]
        [StringLength(50, ErrorMessage ="El nombre puede contener 50 caracteres como maximo.")]
        public string NombreSedeEvento{get; set;} = string.Empty;

        [Required (ErrorMessage ="La ubicacion es obligatoria")]
        [StringLength(100, ErrorMessage ="La ubicacion puede contener 100 caracteres como maximo.")]
        public string Ubicacion{get; set;} = string.Empty;
    }
}