namespace Web.Models;

public class FacturaResponseDto
{
    public int IdFactura { get; set; }
    public int IdUsuario { get; set; }
    public int IdMedioPago { get; set; }
    public decimal Total { get; set; }
    public DateTime FechaFactura { get; set; }
}