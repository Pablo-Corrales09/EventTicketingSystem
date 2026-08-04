using System.ComponentModel.DataAnnotations;
namespace API.DTOs;

public class FacturaCreateDto
{
    [Required]
    public int IdUsuario { get; set; }

    [Required]
    public int IdMedioPago { get; set; }

    [Required]
    public List<int> IdsBoletos { get; set; } = new List<int>();
}