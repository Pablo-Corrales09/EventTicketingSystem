namespace API.Dtos
{
    public class EventoDto : EventoBaseDto
    {
        public int IdEvento{get; set;}
        public List<LocalidadDto> Localidades{get; set;} = new();
    }
}