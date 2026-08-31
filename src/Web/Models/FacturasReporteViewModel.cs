namespace Web.Models;

public class FacturasReporteViewModel
{
    public List<ReporteEventoViewModel> Eventos { get; set; } = new();
    public decimal TotalMes { get; set; }
    public int FacturasMes { get; set; }
    public int BoletosMes { get; set; }
}

public class ReporteEventoViewModel
{
    public string NombreEvento { get; set; } = string.Empty;
    public int CantidadBoletos { get; set; }
    public decimal TotalFacturado { get; set; }
}