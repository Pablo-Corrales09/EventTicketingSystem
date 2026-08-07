namespace Web.Models;

public class EventoSedeViewModel
{
    public int IdEvento { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public DateTime FechaEvento { get; set; }
    public TimeSpan HoraEvento { get; set; }
    public SedeViewModel Sede { get; set; } = new();
    public List<EventoLocalidadViewModel> Localidades { get; set; } = new();
}
