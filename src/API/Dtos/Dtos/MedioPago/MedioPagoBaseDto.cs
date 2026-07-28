using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public abstract class MedioPagoBaseDto
    {
        [Required(ErrorMessage ="El nombre del medio de pago es obligatorio.")]
        [StringLength(50, ErrorMessage = "El nombre no puede exceder los 50 caracteres.")]
        public string NombreMedioPago{get; set;} = string.Empty;       
    }
}