namespace Web.Models;

public class MedioPagoViewModel
{
    public int IdMedioPago { get; set; }
    public string Nombre { get; set; } = string.Empty; // Ejemplo: Tarjeta, Efectivo, SINPE
}