using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class MediosPagoController : ControllerBase
    {
        // Inyección de dependencias del servicio de medios de pago para poder utilizar sus métodos en el controlador.
        private readonly MedioPagoService _medioPagoService;

        // Constructor para inyectar el servicio de medios de pago en el controlador.
        public MediosPagoController(MedioPagoService medioPagoService)
        {
            _medioPagoService = medioPagoService;
        }

        // GET: api/MedioPago
        // Endpoint para obtener todos los medios de pago.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<MedioPagoDto>>> GetMediosPago()
        {
            return Ok(await _medioPagoService.ObtenerTodosLosMediosPagoAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<MedioPagoDto>> GetMedioPago(int id)
        {
            var medioPago = await _medioPagoService.ObtenerMedioPagoPorIdAsync(id);
            return medioPago == null ? NotFound(): Ok(medioPago);
        }
    }
    
}