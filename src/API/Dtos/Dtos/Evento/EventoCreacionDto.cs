namespace API.Dtos
{    
    public class EventoCreacionDto: EventoBaseDto
    {
        public int IdSede { get; set; }

        public IFormFile? ImagenEvento {get; set;}
    }
}