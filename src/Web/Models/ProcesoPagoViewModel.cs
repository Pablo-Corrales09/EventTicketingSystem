namespace Web.Models;

public class ProcesoPagoViewModel
{
    public int IdEventoLocalidad { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public string NombreLocalidad { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int IdUsuario { get; set; } = 1;
    public int IdMedioPago { get; set; }
}