using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class EventosController : ControllerBase
    {
        // Inyección de dependencias del servicio de eventos para poder utilizar sus métodos en el controlador.
        private readonly EventoService _eventoService;

        // Constructor para inyectar el servicio de eventos en el controlador.
        public EventosController(EventoService eventoService)
        {
            _eventoService = eventoService;
        }

        // GET: api/Eventos
        // Endpoint para obtener todos los eventos, sin incluír la información de las localidades asociadas a cada evento.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventoDto>>> GetEventos()
        {
            return Ok(await _eventoService.ObtenerTodosLosEventosAsync());
        }

        // GET: api/Eventos/localidad
        // Endpoint para obtener todos los eventos, incluyendo la información de la sede asociada a cada evento.
        [HttpGet("localidad-sede")]
        public async Task<ActionResult<IEnumerable<EventoSedeDto>>> GetEventosLocalidadSede()
        {
            return Ok(await _eventoService.ObtenerEventosSedeAsync());
        }

        // GET: api/Eventos/mis-eventos?usuarioId={id}
        // Endpoint para obtener los eventos en los que el usuario tiene boletos comprados.
        [HttpGet("mis-eventos")]
        public async Task<ActionResult<IEnumerable<EventoSedeDto>>> GetMisEventos([FromQuery] int usuarioId)
        {
            return Ok(await _eventoService.ObtenerEventosDeUsuarioAsync(usuarioId));
        }


        // GET: api/Eventos/id 
        // Endpoint para obtener un evento por su ID, incluyendo la información de la sede asociada al evento.
        [HttpGet("{id}")]
        public async Task<ActionResult<EventoSedeDto>> GetEventoSede(int id)
        {
            var evento = await _eventoService.ObtenerEventoSedePorIdAsync(id);
            return evento == null ? NotFound() : Ok(evento);
        }

        // POST: api/Eventos/crear
        // Endpoint para insertar un evento nuevo.
        [HttpPost("crear")]
        public async Task<ActionResult<EventoDto>> CrearEvento([FromForm] EventoCreacionDto request)
        {
            var eventoCreado = await _eventoService.CrearEventoAsync(request);

            if (eventoCreado == null)
            {
                return BadRequest("No se pudo crear el evento. Verifica los datos enviados.");
            }
            return Ok(eventoCreado); 
        }


        // PUT: api/Eventos/actualizar/{id}
        // Endpoint para actualizar un evento existente y/o agregarle una imagen.
        [HttpPut("actualizar/{id}")]
        public async Task<ActionResult<EventoDto>> ActualizarEvento(int id, [FromForm] EventoCreacionDto request)
        {
            var eventoActualizado = await _eventoService.ActualizarEventoAsync(id, request);

            if (eventoActualizado == null)
            {
                return NotFound(new { Mensaje = $"No se encontró ningún evento con el ID {id}." });
            }

            return Ok(eventoActualizado);
        }

    }
}


