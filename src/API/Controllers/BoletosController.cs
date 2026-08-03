using Microsoft.AspNetCore.Mvc;
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
        public async Task<ActionResult<IEnumerable<BoletoDto>>> GetBoletos()
        {
            return Ok(await _boletoService.ObtenerTodosAsync());
        }

        [HttpGet("{id}")]
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

        /*[HttpGet("probar-boleto")]
        public async Task<IActionResult> ProbarBoleto(int idLocalidad)
        {
            // Como tu servicio es async, usas 'await'
            string resultado = await _boletoService.CrearNumBoleto(idLocalidad);

            // Retorna el resultado para verlo en el navegador, Postman o Swagger
            return Ok(new { NumeroBoletoGenerado = resultado });
        }*/

        [HttpPost("crear")]
        public async Task<ActionResult<BoletoDto>> CrearBoleto([FromBody] BoletoCreacionDto request)
        {
            var boletoCreado = await _boletoService.CrearBoleto(
                request.IdEventoLocalidad,
                request.IdUsuario,
                request.IdFactura
            );

            if (boletoCreado == null)
            {
                return BadRequest("No se pudo registrar el boleto. Verifica los datos enviados.");
            }

            return CreatedAtAction(nameof(GetBoleto), new { id = boletoCreado.IdBoleto }, boletoCreado);
        }
    }
}