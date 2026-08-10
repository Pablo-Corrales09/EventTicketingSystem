using System.Text.Json.Serialization;

namespace Web.Models;

public class MedioPagoViewModel
{
    public int IdMedioPago { get; set; }

    [JsonPropertyName("NombreMedioPago")]
    public string Nombre { get; set; } = string.Empty;

    public bool EsTarjeta => Nombre.Contains("Tarjeta", StringComparison.OrdinalIgnoreCase);
}
