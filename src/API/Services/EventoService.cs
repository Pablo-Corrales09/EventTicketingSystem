using API.Data;
using API.Dtos;
using API.Models;
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

        private static EventoDto MapearEventoDto(Models.Evento e)
        {
            return new EventoDto
            {
                IdEvento = e.IdEvento,
                NombreEvento = e.NombreEvento,
                FechaEvento = e.FechaEvento,
                HoraEvento = e.HoraEvento,
                Localidades = e.EventoLocalidads != null 
            ? e.EventoLocalidads.Select(el => new LocalidadDto 
              {
                  IdLocalidad = el.IdLocalidad,
                  NombreLocalidad = el.IdLocalidadNavigation != null ? el.IdLocalidadNavigation.NombreLocalidad : string.Empty,
                  
                  EventoLocalidades = new List<EventoLocalidadDto>
                  {
                      new EventoLocalidadDto
                      {
                        IdEventoLocalidad = el.IdEventoLocalidad, 
                        IdEvento = el.IdEvento,                   
                        IdLocalidad = el.IdLocalidad,
                        Precio = el.Precio, 
                        CapacidadDisponible = el.CapacidadDisponible 
                      }
                  }
                    }).ToList()
                : new List<LocalidadDto>()
            };
        }

        public async Task<List<EventoDto>> ObtenerTodosLosEventosAsync()
        {
            var eventos = await _context.Eventos
            .Include (e => e.EventoLocalidads)
            .ThenInclude(el => el.IdLocalidadNavigation)
            .ToListAsync();
            return eventos.Select(e => MapearEventoDto(e)).ToList();
        }
    }
}