using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class LoginRequestDto
    {
        [Required]
        public string? Correo {get; set;} = string.Empty;

        [Required]
        public string? Contrasena {get; set;} = string.Empty;
    }

}