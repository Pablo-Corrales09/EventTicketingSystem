using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public abstract class EventoBaseDto
    {
        [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
        [StringLength(100, ErrorMessage = "El nombre del evento no puede exceder los 100 caracteres.")]
        public string NombreEvento {get; set;} = string.Empty;
        [Required(ErrorMessage = "La fecha es obligatoria")]
        public DateTime FechaEvento{get; set;}
        [Required(ErrorMessage = "La hora del evento es obligatoria.")]
        public TimeOnly HoraEvento{get; set;}
    }
}