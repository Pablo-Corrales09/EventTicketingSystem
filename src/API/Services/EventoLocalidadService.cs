using API.Data;
using API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class EventoLocalidadService
    {
       private readonly DbDevTicketappContext _context;

    // Constructor para inyectar el contexto de la base de datos en el servicio.
       public EventoLocalidadService(DbDevTicketappContext context)
        {
         _context = context;   
        }

        // Método para obtener todas las localidades de eventos de la base de datos y mapearlas a DTOs EventoLocalidadDto.
        public async Task<List<EventoLocalidadDto>> ObtenerEventosLocalidadesAsync()
        {
            var eventoLocalidades = await _context.EventoLocalidads
                .Include(el => el.IdEventoNavigation)  
                .Include(el => el.IdLocalidadNavigation) 
                .ToListAsync();

            return eventoLocalidades.Select(el => MapearEventoLocalidadDto(el)).ToList();
        }

        public async Task<string?> ObtenerNombreEventoPorIdAsync(int idEvento)
        {
            var evento = await _context.Eventos
            .Where(e => e.IdEvento == idEvento)
            .Select(e => e.NombreEvento)
            .FirstOrDefaultAsync();

            return evento;
        }

        // Método para obtener todas las localidades de un evento específico de la base de datos y mapearlas a DTOs EventoLocalidadDto.
        public static EventoLocalidadDto MapearEventoLocalidadDto(Models.EventoLocalidad e)
        {
        return new EventoLocalidadDto
        {
            IdEventoLocalidad = e.IdEventoLocalidad,
            IdEvento = e.IdEvento,
            NombreEvento = e.IdEventoNavigation != null ? e.IdEventoNavigation.NombreEvento : string.Empty, 
            IdLocalidad = e.IdLocalidad, 
            NombreLocalidad = e.IdLocalidadNavigation != null ? e.IdLocalidadNavigation.NombreLocalidad : string.Empty,
            CapacidadDisponible = e.CapacidadDisponible,
            Precio = e.Precio
        }; 
        }
    }// Fin de la clase.
}// Fin del namespace.