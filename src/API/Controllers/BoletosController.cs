using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BoletosController : ControllerBase
    {
        private readonly BoletoService _boletoService;

        public BoletosController(BoletoService boletoService)
        {
            _boletoService = boletoService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<BoletoDto>>> GetBoletos(
            int? usuarioId = null,
            string? numeroBoleto = null,
            string? nombreEvento = null,
            string? nombreCliente = null)
        {
            return Ok(await _boletoService.ObtenerTodosAsync(
                usuarioId,
                numeroBoleto,
                nombreEvento,
                nombreCliente));
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<BoletoDto>> GetBoleto(int id)
        {
            var boleto = await _boletoService.ObtenerBoletoPorIdAsync(id);
            return boleto == null ? NotFound() : Ok(boleto);
        }

        [HttpGet("buscarNumero")]
        public async Task<ActionResult<BoletoDto>> BuscarPorNumero([FromQuery] string numero)
        {
            var boleto = await _boletoService.BuscarPorNumeroAsync(numero);
            return boleto == null ? NotFound() : Ok(boleto);
        }

        [Authorize]
        [EnableRateLimiting("CompraUsuario")]
        [HttpPost("comprar")]
        public async Task<ActionResult<BoletoDto>> CompraDeBoleto([FromBody] BoletoCreacionDto request)
        {
            var boletoCreado = await _boletoService.ComprarBoleto(
                request.IdEventoLocalidad,
                request.IdUsuario,
                request.IdMedioPago,
                request.Cantidad
            );

            if (boletoCreado == null)
            {
                return BadRequest("No se pudo registrar el boleto. Verifica los datos enviados.");
            }

            return CreatedAtAction(nameof(GetBoleto), new { id = boletoCreado.IdBoleto }, boletoCreado);
        }
    }
}