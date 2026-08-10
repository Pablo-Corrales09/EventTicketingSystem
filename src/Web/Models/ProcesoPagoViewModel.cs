namespace Web.Models;

public class ProcesoPagoViewModel
{
    public int IdEvento { get; set; }
    public int IdEventoLocalidad { get; set; }
    public string NombreEvento { get; set; } = string.Empty;
    public string NombreLocalidad { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int Cantidad { get; set; } = 1;
    public decimal Total => Precio * Cantidad;
    public int IdUsuario { get; set; }
    public int IdMedioPago { get; set; }
}
