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


        // GET: api/Eventos/id 
        // Endpoint para obtener un evento por su ID, incluyendo la información de la sede asociada al evento.
        [HttpGet("{id}")]
        public async Task<ActionResult<EventoSedeDto>> GetEventoSede(int id)
        {
            var evento = await _eventoService.ObtenerEventoSedePorIdAsync(id);
            return evento == null ? NotFound() : Ok(evento);
        }

    }
}