namespace API.Dtos
{
    public class EventoLocalidadDto: EventoLocalidadBaseDto
    {
        public int IdEventoLocalidad{get; set;}

        public int IdEvento{get; set;}

        public string? NombreEvento { get; set; }
        
        public int IdLocalidad{get; set;}
        public string? NombreLocalidad { get; set; }
    }
}