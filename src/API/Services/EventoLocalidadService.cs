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
            return await ObtenerQueryBase()
                .Select(el => MapearEventoLocalidadDto(el))
                .ToListAsync();
        }

        // Método auxiliar para obtener el nombre de un evento específico por su ID desde la base de datos.
        public async Task<string?> ObtenerNombreEventoPorIdAsync(int idEvento)
        {
            var evento = await _context.Eventos
            .Where(e => e.IdEvento == idEvento)
            .Select(e => e.NombreEvento)
            .FirstOrDefaultAsync();

            return evento;
        }

          // Método para obtener un evento-localidad específico por su ID desde la base de datos y mapearlo a un DTO EventoLocalidadDto.
        public async Task<EventoLocalidadDto?> ObtenerEventoLocalidadPorIdAsync(int id)
        {
            return await ObtenerQueryBase()
                .Where(el => el.IdEventoLocalidad == id)
                .Select(el => MapearEventoLocalidadDto(el))
                .FirstOrDefaultAsync();      
        }

        // Método auxiliar para construir la consulta base que incluye las relaciones necesarias para obtener los datos de eventos y localidades.

        private IQueryable<Models.EventoLocalidad> ObtenerQueryBase()
        {
            return _context.EventoLocalidads
                .Include(el => el.IdEventoNavigation)
                .Include(el => el.IdLocalidadNavigation);
        }

        // Método auxiliar para obtener todas las localidades de un evento específico de la base de datos y mapearlas a DTOs EventoLocalidadDto.
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