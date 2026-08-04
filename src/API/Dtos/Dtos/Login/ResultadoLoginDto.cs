using System.ComponentModel.DataAnnotations;

namespace API.Dtos
{
    public class ResultadoLoginDto
    {
        public string Token {get; set;} = string.Empty;
        public DateTime Expiracion{ get; set;}
    }
}