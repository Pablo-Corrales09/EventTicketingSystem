using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public abstract class EventoLocalidadBaseDto
    {
    [Required(ErrorMessage ="El precio es obligatorio.") ]
    public decimal? Precio { get; set; }

    [Required(ErrorMessage = "Debe ingresar la capacidad inicial para crear el evento.")]
    [Range(1, 80000, ErrorMessage ="La capacidad debe ser entre 1 y  80.000 espectadores.")]
    public int? CapacidadDisponible { get; set; }        
    }
}