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
        public async Task<List<BoletoDto>> ObtenerTodosAsync(
            int? usuarioId = null,
            string? numeroBoleto = null,
            string? nombreEvento = null,
            string? nombreCliente = null)
        {
            var query = ObtenerQueryBase();

            if (usuarioId.HasValue)
            {
                query = query.Where(b => b.IdUsuario == usuarioId.Value);
            }

            if (!string.IsNullOrWhiteSpace(numeroBoleto))
            {
                query = query.Where(b => b.NumBoleto.Contains(numeroBoleto));
            }

            if (!string.IsNullOrWhiteSpace(nombreEvento))
            {
                query = query.Where(b =>
                    b.IdEventoLocalidadNavigation != null &&
                    b.IdEventoLocalidadNavigation.IdEventoNavigation != null &&
                    b.IdEventoLocalidadNavigation.IdEventoNavigation.NombreEvento.Contains(nombreEvento));
            }

            if (!string.IsNullOrWhiteSpace(nombreCliente))
            {
                query = query.Where(b =>
                    b.IdUsuarioNavigation != null &&
                    (b.IdUsuarioNavigation.Nombre + " " + b.IdUsuarioNavigation.Apellido).Contains(nombreCliente));
            }

            return await query
                .Select(b => MapearBoletoDto(b))
                .ToListAsync();
        }

        // Método para comprar una cantidad de boletos de una localidad, generando una única factura.
        public async Task<BoletoDto?> ComprarBoleto(int IdEventoLocalidad, int IdUsuario, int IdMedioPago, int cantidad)
        {
            cantidad = Math.Max(1, cantidad);

            using var transaction = await _context.Database.BeginTransactionAsync();

            try
            {
                var localidadEvento = await _context.EventoLocalidads
                    .FromSqlRaw(
                        "SELECT * FROM evento_localidad WITH (UPDLOCK, ROWLOCK) WHERE id_evento_localidad = {0}",
                        IdEventoLocalidad)
                    .FirstOrDefaultAsync();

                if (localidadEvento == null)
                {
                    throw new KeyNotFoundException("La localidad seleccionada no existe.");
                }

                if (cantidad > localidadEvento.CapacidadDisponible)
                {
                    throw new InvalidOperationException("No hay suficientes boletos disponibles en la localidad seleccionada.");
                }

                var datosEvento = await _context.EventoLocalidads
                    .Where(el => el.IdEventoLocalidad == IdEventoLocalidad)
                    .Select(el => new
                    {
                        el.IdEventoNavigation.NombreEvento,
                        el.IdEventoNavigation.FechaEvento
                    })
                    .FirstOrDefaultAsync();

                var prefijo = ObtenerPrefijo(datosEvento?.NombreEvento);

                DateTime? fechaCompra = (datosEvento != null && DateTime.Now <= datosEvento.FechaEvento)
                    ? DateTime.Now
                    : null;

                var facturaDto = new FacturaCreateDto
                {
                    IdUsuario = IdUsuario,
                    IdMedioPago = IdMedioPago,
                    IdsBoletos = Enumerable.Repeat(IdEventoLocalidad, cantidad).ToList()
                };

                //Se crea la factura con el total calculado (precio * cantidad).
                var facturaCreada = await _facturaService.CrearFacturaAsync(facturaDto);

                if (facturaCreada == null)
                {
                    throw new Exception("No se pudo generar la factura.");
                }

                //Consecutivo base para numerar los boletos.
                int consecutivoBase = await _context.Boletos.CountAsync();

                for (int i = 0; i < cantidad; i++)
                {
                    var nuevoBoleto = new Boleto
                    {
                        IdEventoLocalidad = IdEventoLocalidad,
                        IdUsuario = IdUsuario,
                        IdFactura = facturaCreada.IdFactura,
                        NumBoleto = $"BOL-{prefijo}-{IdEventoLocalidad}-{consecutivoBase + i + 1}",
                        FechaCompra = fechaCompra
                    };

                    _context.Add(nuevoBoleto);
                }

                localidadEvento.CapacidadDisponible -= cantidad;

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                return await ObtenerQueryBase()
                    .Where(b => b.IdFactura == facturaCreada.IdFactura)
                    .OrderBy(b => b.IdBoleto)
                    .Select(b => MapearBoletoDto(b))
                    .FirstOrDefaultAsync();
            }
            catch (Exception)
            {
                await transaction.RollbackAsync();
                throw;
            }
        }

        //Método auxiliar que calcula el prefijo del número de boleto a partir del nombre del evento.
        private static string ObtenerPrefijo(string? nombreEvento)
        {
            if (string.IsNullOrEmpty(nombreEvento))
            {
                return "XX";
            }

            return nombreEvento.Length >= 2
                ? nombreEvento.Substring(0, 2).ToUpper()
                : nombreEvento.ToUpper();
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

       //Funcion auxiliar para crear el numero de boleto de manera consecutiva, segun el evento al que corresponde.  
       private async Task<string> CrearNumBoleto(int idEventoLocalidad, int consecutivo)
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

            //Se incluye el IdEventoLocalidad en el número para garantizar unicidad global (columna UNIQUE).
            var numBoleto = $"BOL-{prefijo}-{idEventoLocalidad}-{consecutivo}";
            
            return numBoleto;
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