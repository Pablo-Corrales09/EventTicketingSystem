namespace API.Dtos
{
    public class EventoSedeDto : EventoBaseDto
    {
        public int IdEvento{get; set;}

        public SedeUbicacionSimpleDto? Sede { get; set; }
        
    }
}