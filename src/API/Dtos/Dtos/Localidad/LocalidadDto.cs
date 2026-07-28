namespace API.Dtos
{
    public class LocalidadDto : LocalidadBaseDto
    {
        public int IdLocalidad { get; set; }
        public List<EventoLocalidadDto> EventoLocalidades { get; set; } = new();
    }
}