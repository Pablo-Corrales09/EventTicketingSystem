using API.Data;
using API.Dtos;
using Microsoft.EntityFrameworkCore;
namespace API.Services
{
    public class EventoService
    {
        public readonly DbDevTicketappContext _context;

        public EventoService(DbDevTicketappContext context)
        {
            _context = context;
        }    

        //Función para obtener los eventos de la DB y maperalos a un EventoDto, sin incluír la información de las localidades asociadas al evento.
            public async Task<List<EventoDto>> ObtenerTodosLosEventosAsync()
        {
            var eventos = await _context.Eventos
            .ToListAsync();
            return eventos.Select(e => MapearEventoLocalidadDto(e)).ToList();
        }

        //Método auxiliar para evitar la duplicación de código al mapear un objeto Evento a EventoDto
        private static EventoDto MapearEventoLocalidadDto(Models.Evento e)
        {
            return new EventoDto
            {
                IdEvento = e.IdEvento,
                NombreEvento = e.NombreEvento,
                FechaEvento = e.FechaEvento,
                HoraEvento = e.HoraEvento,                    
            };

        }

        //Función para obtener los eventos de la DB y maperalos a un EventoSedeDto, incluyendo la información de la sede asociada al evento.
            public async Task<List<EventoSedeDto>> ObtenerEventosSedeAsync()
        {
            var eventos = await _context.Eventos
            .Include(e => e.IdSedeNavigation)
            .ToListAsync();
            return eventos.Select(e => MapearEventoSedeDto(e)).ToList();
        }

        //Función para obtener un evento por su ID de la DB y mapearlo a un EventoSedeDto, incluyendo la información de la sede asociada al evento.
        public async Task<EventoSedeDto?> ObtenerEventoSedePorIdAsync(int id)
        {
            return await _context.Eventos
            .Where(e => e.IdEvento == id)
            .Include(e => e.IdSedeNavigation)
            .Select(e => MapearEventoSedeDto(e))
            .FirstOrDefaultAsync();
        }

        //Método auxiliar para evitar la duplicación de código al mapear un objeto Evento a EventoSedeDto
        private static EventoSedeDto MapearEventoSedeDto(Models.Evento e)
        {
            return new EventoSedeDto
            {
                IdEvento = e.IdEvento,
                NombreEvento = e.NombreEvento,
                FechaEvento = e.FechaEvento,
                HoraEvento = e.HoraEvento,
                Sede = e.IdSedeNavigation != null ? new SedeUbicacionSimpleDto
                {
                    IdSedeEvento = e.IdSedeNavigation.IdSedeEvento,
                    NombreSedeEvento = e.IdSedeNavigation.NombreSedeEvento,
                    Ubicacion = e.IdSedeNavigation.Ubicacion
                } : null,
            };
        }

     
    }
}