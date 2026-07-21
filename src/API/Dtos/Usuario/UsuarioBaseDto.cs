namespace API.Dtos
{
    public class UsuarioBaseDto
    {
        public string Nombre { get; set; } = null!;
        public string Apellido { get; set; } = null!;
        public string Correo { get; set; } = null!;
        public string? Telefono { get; set; }
        public int? IdRole { get; set; }
    }
}