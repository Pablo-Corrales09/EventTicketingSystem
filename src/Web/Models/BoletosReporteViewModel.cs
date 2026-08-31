namespace Web.Models;

public class BoletosReporteViewModel
{
    public List<ReporteEventoViewModel> Eventos { get; set; } = new();
    public decimal TotalMes { get; set; }
    public int BoletosMes { get; set; }
    public int EventosMes { get; set; }
}