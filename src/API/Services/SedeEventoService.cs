using API.Data;
using API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class SedeEventoService
    {
        private readonly DbDevTicketappContext _context;

        public SedeEventoService(DbDevTicketappContext context)
        {
            _context = context;
        }

        // Función para obtener todas las sedes de eventos de la DB mapeadas a un DTO simple.
        public async Task<List<SedeUbicacionSimpleDto>> ObtenerTodasAsync()
        {
            return await _context.SedeEventos
                .OrderBy(s => s.NombreSedeEvento)
                .Select(s => new SedeUbicacionSimpleDto
                {
                    IdSedeEvento = s.IdSedeEvento,
                    NombreSedeEvento = s.NombreSedeEvento,
                    Ubicacion = s.Ubicacion
                })
                .ToListAsync();
        }
    }
}
