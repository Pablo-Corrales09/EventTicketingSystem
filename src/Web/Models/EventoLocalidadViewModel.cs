namespace Web.Models;

public class EventoLocalidadViewModel
{
    public int IdEventoLocalidad { get; set; }
    public int IdEvento { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public int IdLocalidad { get; set; }
    public string NombreLocalidad { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int CapacidadDisponible { get; set; }
}