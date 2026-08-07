using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using API.Services;
using API.DTOs;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class FacturasController : ControllerBase
    {
        private readonly FacturaService _facturaService;

        public FacturasController(FacturaService facturaService)
        {
            _facturaService = facturaService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<FacturaResponseDto>>> GetFacturas()
        {
            return Ok(await _facturaService.ObtenerTodasAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<FacturaResponseDto>> GetFactura(int id)
        {
            var factura = await _facturaService.ObtenerFacturaPorIdAsync(id);
            return factura == null ? NotFound() : Ok(factura);
        }

        [HttpPost("crear")]
        public async Task<ActionResult<FacturaResponseDto>> CrearFactura([FromBody] FacturaCreateDto request)
        {
            
            var facturaCreada = await _facturaService.CrearFacturaAsync(request);

            if (facturaCreada == null)
            {
                return BadRequest("No se pudo generar la factura. Verifica los datos enviados.");
            }
            return CreatedAtAction(nameof(GetFactura), new { id = facturaCreada.IdFactura }, facturaCreada);
        }
    }
}