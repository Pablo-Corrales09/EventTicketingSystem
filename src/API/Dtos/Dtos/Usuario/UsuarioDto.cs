namespace API.Dtos
{
    public class UsuarioDto : UsuarioBaseDto
    {
        public int IdUsuario { get; set; }
        public string? NombreRole { get; set; }
    }
}