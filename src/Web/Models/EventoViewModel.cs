namespace Web.Models;

public class EventoViewModel
{
    public int IdEvento { get; set; }

    public string NombreEvento { get; set; } = string.Empty;

    public DateTime FechaEvento { get; set; }

    public TimeOnly HoraEvento { get; set; }

    public SedeViewModel? Sede { get; set; }
}

public class SedeViewModel
{
    public int IdSedeEvento { get; set; }

    public string NombreSedeEvento { get; set; } = string.Empty;

    public string Ubicacion { get; set; } = string.Empty;
}