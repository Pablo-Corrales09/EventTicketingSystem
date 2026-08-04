using API.Data;
using API.Dtos;
using API.DTOs;
using API.Models;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class FacturaService
    {
        private readonly DbDevTicketappContext _context;

        public FacturaService(DbDevTicketappContext context)
        {
            _context = context;
        }

        public async Task<List<FacturaResponseDto>> ObtenerTodasAsync()
        {
            return await ObtenerQueryBase()
                .Select(f => MapearFacturaDto(f))
                .ToListAsync();
        }

        // Método para crear la factura
        public async Task<FacturaResponseDto?> CrearFacturaAsync(FacturaCreateDto request)
        {
            decimal totalCalculado = await CalcularTotalFactura(request.IdsBoletos);

            var nuevaFactura = new Factura
            {
                IdUsuario = request.IdUsuario,
                IdMedioPago = request.IdMedioPago,
                FechaFactura = DateTime.Now,
                NumeroFactura = await CrearNumFactura(),
                Total = totalCalculado
            };

            _context.Facturas.Add(nuevaFactura);
            await _context.SaveChangesAsync();
            return await ObtenerFacturaPorIdAsync(nuevaFactura.IdFactura);
        }

        public async Task<FacturaResponseDto?> ObtenerFacturaPorIdAsync(int idFactura)
        {
            return await ObtenerQueryBase()
                .Where(f => f.IdFactura == idFactura)
                .Select(f => MapearFacturaDto(f))
                .FirstOrDefaultAsync();
        }

        // Función auxiliar para calcular el total sumando el precio de las localidades
        private async Task<decimal> CalcularTotalFactura(List<int> idsEventoLocalidad)
        {
            if (idsEventoLocalidad == null || !idsEventoLocalidad.Any())
                return 0;

            decimal total = await _context.EventoLocalidads
                .Where(el => idsEventoLocalidad.Contains(el.IdEventoLocalidad))
                .SumAsync(el => el.Precio);

            return total;
        }

        // Función auxiliar para crear el número de factura de manera consecutiva
        private async Task<string> CrearNumFactura()
        {
            var consecutivo = await ObtenerConsecutivo();
            
            string anioActual = DateTime.Now.Year.ToString();
            var numFactura = $"FAC-{anioActual}-{consecutivo:D6}";
            
            return numFactura;
        }

        // Función auxiliar para obtener el consecutivo general de facturas
        private async Task<int> ObtenerConsecutivo()
        {
            int cantidadFacturas = await _context.Facturas.CountAsync();
            return cantidadFacturas + 1;
        }

        // Método privado base que incluye las relaciones necesarias, incluyendo las anidadas de los boletos
        private IQueryable<Factura> ObtenerQueryBase()
        {
            return _context.Facturas
                .Include(f => f.IdUsuarioNavigation)
                .Include(f => f.IdMedioPagoNavigation)
                .Include(f => f.Boletos) 
                    .ThenInclude(b => b.IdEventoLocalidadNavigation) 
                        .ThenInclude(el => el.IdEventoNavigation) 
                .Include(f => f.Boletos)
                    .ThenInclude(b => b.IdEventoLocalidadNavigation)
                        .ThenInclude(el => el.IdLocalidadNavigation); 
        }

        // Método centralizado para no repetir el código del mapeo
        private static FacturaResponseDto MapearFacturaDto(Factura f)
        {
            return new FacturaResponseDto
            {
                IdFactura = f.IdFactura,
                IdUsuario = f.IdUsuario,
                IdMedioPago = f.IdMedioPago,
                FechaFactura = f.FechaFactura,
                NumeroFactura = f.NumeroFactura,
                Total = f.Total,
            
                Boletos = f.Boletos.Select(b => new BoletoDto
                {
                    IdBoleto = b.IdBoleto,
                    NumBoleto = b.NumBoleto,
                    FechaCompra = b.FechaCompra != null ? DateOnly.FromDateTime(b.FechaCompra.Value) : null,
                    NumeroFactura = f.NumeroFactura,
                    
                    NombreLocalidad = b.IdEventoLocalidadNavigation?.IdLocalidadNavigation?.NombreLocalidad ?? string.Empty,
                    NombreEvento = b.IdEventoLocalidadNavigation?.IdEventoNavigation?.NombreEvento ?? string.Empty,
                    FechaEvento = b.IdEventoLocalidadNavigation?.IdEventoNavigation != null 
                        ? DateOnly.FromDateTime(b.IdEventoLocalidadNavigation.IdEventoNavigation.FechaEvento) 
                        : null,
                    HoraEvento = b.IdEventoLocalidadNavigation?.IdEventoNavigation?.HoraEvento,
                    Precio = b.IdEventoLocalidadNavigation?.Precio ?? 0,
                    
                    NombreUsuario = f.IdUsuarioNavigation?.Nombre ?? string.Empty,
                    ApellidoUsuario = f.IdUsuarioNavigation?.Apellido ?? string.Empty,
                    CorreoUsuario = f.IdUsuarioNavigation?.Correo ?? string.Empty,
                    TelefonoUsuario = f.IdUsuarioNavigation?.Telefono ?? string.Empty
                }).ToList()
            };
        }
    }
}