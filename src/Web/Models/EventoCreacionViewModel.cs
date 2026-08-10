using System.ComponentModel.DataAnnotations;

namespace Web.Models;

public class EventoCreacionViewModel
{
    public int IdEvento { get; set; }

    [Required(ErrorMessage = "El nombre del evento es obligatorio.")]
    [StringLength(100, ErrorMessage = "El nombre del evento no puede exceder los 100 caracteres.")]
    public string NombreEvento { get; set; } = string.Empty;

    [Required(ErrorMessage = "La fecha es obligatoria.")]
    public DateTime FechaEvento { get; set; }

    [Required(ErrorMessage = "La hora del evento es obligatoria.")]
    public TimeOnly HoraEvento { get; set; }

    [Required(ErrorMessage = "Debes seleccionar una sede.")]
    public int IdSede { get; set; }

    public IFormFile? ImagenEvento { get; set; }

    public string? ImagenUrl { get; set; }

    public List<SedeViewModel> Sedes { get; set; } = new();
}
