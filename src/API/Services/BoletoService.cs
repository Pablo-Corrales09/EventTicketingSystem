using API.Data;
using API.Dtos;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class BoletoService
    {
        private readonly DbDevTicketappContext _context;

        public BoletoService(DbDevTicketappContext context)
        {
            _context = context;
        }

        

        // Método que devuelve toda la lista de boletos con su respectiva información de usuario.
        public async Task<List<BoletoDto>> ObtenerTodosAsync()
        {
            return await ObtenerQueryBase()
                .Select(b => MapearBoletoDto(b))
                .ToListAsync();
        }

        // Método para obtener un boleto por su ID reutilizando la query base y el mapeador
        public async Task<BoletoDto?> ObtenerBoletoPorIdAsync(int idBoleto)
        {
            return await ObtenerQueryBase()
                .Where(b => b.IdBoleto == idBoleto)
                .Select(b => MapearBoletoDto(b))
                .FirstOrDefaultAsync();
        }

        // Método para buscar por número de boleto incluyendo al usuario
        public async Task<BoletoDto?> BuscarPorNumeroAsync(string numero)
        {
            return await ObtenerQueryBase()
                .Where(b => b.NumBoleto == numero)
                .Select(b => MapearBoletoDto(b))
                .FirstOrDefaultAsync();
        }

        public async Task<string?> ObtenerNombreClientePorIdAsync(int idUsuario)
        {
            return await ObtenerQueryBase()
                .Where(b => b.IdUsuario == idUsuario)
                .Select(b => b.IdUsuarioNavigation != null 
                    ? b.IdUsuarioNavigation.Nombre + " " + b.IdUsuarioNavigation.Apellido 
                    : null)
                .FirstOrDefaultAsync();
        }

        // Método privado base que incluye las relaciones necesarias
        private IQueryable<Models.Boleto> ObtenerQueryBase()
        {
            return _context.Boletos
                .Include(b => b.IdEventoLocalidadNavigation)
                    .ThenInclude(el => el.IdLocalidadNavigation)
                .Include(b => b.IdEventoLocalidadNavigation)
                    .ThenInclude(el => el.IdEventoNavigation)
                .Include(b => b.IdFacturaNavigation)
                .Include(b => b.IdUsuarioNavigation)
                    .ThenInclude(u => u.IdRoleNavigation); 
        }

        // Método centralizado para no repetir el código del mapeo
        private static BoletoDto MapearBoletoDto(Models.Boleto b)
        {
            return new BoletoDto
            {
                IdBoleto = b.IdBoleto,
                NumBoleto = b.NumBoleto,
                FechaCompra = b.FechaCompra != null ? DateOnly.FromDateTime(b.FechaCompra.Value): null,
                
                //Mapeo de la factura asociada al boleto.                
                NumeroFactura = b.IdFacturaNavigation != null ? b.IdFacturaNavigation.NumeroFactura : string.Empty,

                // Mapeo de Localidad
                NombreLocalidad = b.IdEventoLocalidadNavigation != null && b.IdEventoLocalidadNavigation.IdLocalidadNavigation != null 
                    ? b.IdEventoLocalidadNavigation.IdLocalidadNavigation.NombreLocalidad 
                    : string.Empty,

                // Mapeo del evento asociado al boleto
                NombreEvento = b.IdEventoLocalidadNavigation != null && b.IdEventoLocalidadNavigation.IdEventoNavigation != null 
                    ? b.IdEventoLocalidadNavigation.IdEventoNavigation.NombreEvento 
                    : string.Empty,
                    
                FechaEvento = b.IdEventoLocalidadNavigation != null && b.IdEventoLocalidadNavigation.IdEventoNavigation != null 
                    ? DateOnly.FromDateTime(b.IdEventoLocalidadNavigation.IdEventoNavigation.FechaEvento)
                    : null,
                    
                HoraEvento = b.IdEventoLocalidadNavigation != null && b.IdEventoLocalidadNavigation.IdEventoNavigation != null 
                    ? b.IdEventoLocalidadNavigation.IdEventoNavigation.HoraEvento 
                    : null,

                //Mapeo del precio del boleto desde la localidad asociada al evento
                Precio = b.IdEventoLocalidadNavigation != null ? b.IdEventoLocalidadNavigation.Precio : 0,

                // Mapeo del Usuario...
                NombreUsuario = b.IdUsuarioNavigation != null ? b.IdUsuarioNavigation.Nombre : string.Empty,
                ApellidoUsuario = b.IdUsuarioNavigation != null ? b.IdUsuarioNavigation.Apellido : string.Empty,
                CorreoUsuario = b.IdUsuarioNavigation != null ? b.IdUsuarioNavigation.Correo : string.Empty,
                TelefonoUsuario = b.IdUsuarioNavigation != null ? b.IdUsuarioNavigation.Telefono ?? string.Empty : string.Empty
            };
        }

        public async Task<BoletoDto?> CrearBoleto(int IdBoleto, int IdEventoLocalidad, int IdFactura, int IdUsuario, string numBoleto, DateTime? FechaCompra)
        {
            var nuevoBoleto = new Boleto
            {
                IdBoleto = IdBoleto,
                IdEventoLocalidad = IdEventoLocalidad,
                IdFactura = IdFactura,
                IdUsuario = IdUsuario,
                NumBoleto = numBoleto,
                FechaCompra = FechaCompra
            };

            _context.Add(nuevoBoleto);
            await _context.SaveChangesAsync();

            // Para que devuelva el boleto creado con los datos del usuario cargados, 
            // podemos consultarlo de nuevo usando el ID y la query base:
            return await ObtenerBoletoPorIdAsync(nuevoBoleto.IdBoleto);
        }
    }
}