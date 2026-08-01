namespace API.Dtos
{
    public class SedeUbicacionSimpleDto
    {
        public int IdSedeEvento { get; set; }
        public string NombreSedeEvento { get; set; } = string.Empty;
        public string Ubicacion { get; set; } = string.Empty;

    }
}