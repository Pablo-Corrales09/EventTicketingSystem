using API.Data;
using API.Dtos;
using API.DTOs;
using API.Models;
using Microsoft.AspNetCore.DataProtection.Repositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Abstractions;

namespace API.Services
{
    public class BoletoService
    {
        private readonly DbDevTicketappContext _context;
        private readonly FacturaService _facturaService;

        public BoletoService(DbDevTicketappContext context, FacturaService facturaService)
        {
            _context = context;
            _facturaService = facturaService;
        }      

        // Método que devuelve toda la lista de boletos con su respectiva información de usuario.
        public async Task<List<BoletoDto>> ObtenerTodosAsync()
        {
            return await ObtenerQueryBase()
                .Select(b => MapearBoletoDto(b))
                .ToListAsync();
        }

        //Pendiente asignar factura
        public async Task<BoletoDto?> ComprarBoleto(int IdEventoLocalidad, int IdUsuario, int IdMedioPago)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var facturaDto = new FacturaCreateDto
                {
                    IdUsuario = IdUsuario,
                    IdMedioPago = IdMedioPago,
                    IdsBoletos = new List<int> { IdEventoLocalidad } 
                };

                // 2. Creamos la factura
                var facturaCreada = await _facturaService.CrearFacturaAsync(facturaDto);

                if (facturaCreada == null)
                {
                    throw new Exception("No se pudo generar la factura.");
                }

                var nuevoBoleto = new Boleto
                {
                    IdEventoLocalidad = IdEventoLocalidad,               
                    IdUsuario = IdUsuario,
                    IdFactura = facturaCreada.IdFactura,
                    NumBoleto = await CrearNumBoleto(IdEventoLocalidad),
                    FechaCompra = await AsignarFechaCompra(IdEventoLocalidad)
                };

                _context.Add(nuevoBoleto);
                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerBoletoPorIdAsync(nuevoBoleto.IdBoleto);
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw; 
            }
        }

       
       //Método auxiliar que valida la fecha de compra: si no es mayor al evento asigna la fecha actual, si no, le asigna nulo.
       private async Task<DateTime?> AsignarFechaCompra(int IdEventoLocalidad)
        {
            bool esValida = await ValidarFechaEvento(IdEventoLocalidad);

            if (esValida)
            {
                return  DateTime.Now;
            }
            return null;
        }
       
        //Método auxiliar para validar que la fecha de compra del boleto no sea posterior a la fecha del evento.
        private async Task<bool> ValidarFechaEvento(int idEventoLocalidad)
        {
            var fechaEvento = await _context.EventoLocalidads
                .Where(el => el.IdEventoLocalidad == idEventoLocalidad)
                .Include(ev => ev.IdEventoNavigation)
                .Select(el => el.IdEventoNavigation.FechaEvento)
                .FirstOrDefaultAsync();
            var fechaActual = DateTime.Now;
            return fechaActual <= fechaEvento;
        }

       //Funcion auxiliar para crear el nunero de boleto de manera consecutiva, segun el evento al que corresponde.  
       private async Task<string> CrearNumBoleto(int idEventoLocalidad)
        {
            var localidad = await _context.EventoLocalidads
                .Where(el => el.IdEventoLocalidad == idEventoLocalidad)
                .Include(ev => ev.IdEventoNavigation)
                .Select(el => el.IdEventoNavigation.NombreEvento)
                .FirstOrDefaultAsync();

            string prefijo = "XX"; 
            if (!string.IsNullOrEmpty(localidad))
            {
                if (localidad.Length >= 2)
                {
                    prefijo = localidad.Substring(0, 2).ToUpper();
                }
                else
                {
                    prefijo = localidad.ToUpper();
                }
            }
            var consecutivo = await ObtenerConsecutivo(idEventoLocalidad);

            var numBoleto = $"BOL-{prefijo}-{consecutivo}";
            
            return numBoleto;
        }

        //Funcion auxiliar para obtener el numero de consecutivo de los tickets vendidos de un evento especifico.
        private async Task<int> ObtenerConsecutivo(int idEventoLocalidad)
        {

            int cantidadBoletos = await _context.Boletos
            .Where(b => b.IdEventoLocalidad == idEventoLocalidad)
            .CountAsync();
            
            int consecutivo = cantidadBoletos + 1;

            return consecutivo;
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

        
    }
}