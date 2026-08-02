using API.Data;
using API.Dtos;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class MedioPagoService
    {
        private readonly DbDevTicketappContext _context;

        public MedioPagoService(DbDevTicketappContext context)
        {
            _context = context;
        }

        public async Task<MedioPagoDto?> ObtenerMedioPagoPorIdAsync(int id)
        {
            return await _context.MedioPagos
            .Where(m => m.IdMedioPago == id)
            .Select(m => MapearMedioPagoDto(m))
            .FirstOrDefaultAsync();
        }
        
        //Función para obtener los medios de pago de la DB y maperalos a un MedioPagoDto.   
        public async Task<List<MedioPagoDto>> ObtenerTodosLosMediosPagoAsync()
        {
            var mediosPago = await _context.MedioPagos
            .ToListAsync();
            return mediosPago.Select(m => MapearMedioPagoDto(m)).ToList();            
        }

        
        //Método auxiliar para evitar la duplicación de código al mapear un objeto MedioPago a MedioPagoDto
        public static MedioPagoDto MapearMedioPagoDto(Models.MedioPago m)
        {
           return new MedioPagoDto
           {
               IdMedioPago = m.IdMedioPago,
               NombreMedioPago = m.NombreMedioPago
           }; 
        }
    }

}