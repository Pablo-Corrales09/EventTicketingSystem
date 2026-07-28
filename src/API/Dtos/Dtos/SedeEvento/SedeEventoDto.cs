namespace API.Dtos
{
    public class SedeEventoDto
    {
        public int IdSedeEvento{get; set;}
        public List<LocalidadDto> Localidades { get; set; } = new List<LocalidadDto>(); 
    }
}