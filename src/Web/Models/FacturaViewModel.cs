namespace Web.Models;

public class FacturaViewModel
{
    public int IdFactura { get; set; }

    public int IdUsuario { get; set; }

    public int IdMedioPago { get; set; }

    public DateTime FechaFactura { get; set; }

    public string NumeroFactura { get; set; } = string.Empty;

    public decimal Total { get; set; }

    public List<BoletoViewModel> Boletos { get; set; } = new();
}

public class BoletoViewModel
{
    public int IdBoleto { get; set; }

    public string? NumBoleto { get; set; }

    public DateOnly? FechaCompra { get; set; }

    public string? NumeroFactura { get; set; }

    public decimal Precio { get; set; }

    public string NombreEvento { get; set; } = string.Empty;

    public DateOnly? FechaEvento { get; set; }

    public TimeOnly? HoraEvento { get; set; }

    public string NombreLocalidad { get; set; } = string.Empty;

    public string NombreUsuario { get; set; } = string.Empty;

    public string ApellidoUsuario { get; set; } = string.Empty;

    public string CorreoUsuario { get; set; } = string.Empty;

    public string TelefonoUsuario { get; set; } = string.Empty;
}