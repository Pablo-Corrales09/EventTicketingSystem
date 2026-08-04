using API.Dtos;

namespace API.DTOs;

public class FacturaResponseDto
{
    public int IdFactura { get; set; }
    public int IdUsuario { get; set; }
    public int IdMedioPago { get; set; }
    public DateTime? FechaFactura { get; set; }
    public string NumeroFactura { get; set; } = string.Empty;
    public decimal Total { get; set; }
    public List<BoletoDto> Boletos { get; set; } = new List<BoletoDto>();
}