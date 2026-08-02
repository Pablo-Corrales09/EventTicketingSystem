using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventosLocalidadesController : ControllerBase
    {
        private readonly EventoLocalidadService _eventoLocalidadService;

        public EventosLocalidadesController(EventoLocalidadService eventoLocalidadService)
        {
            _eventoLocalidadService = eventoLocalidadService;
        }

        // GET: api/EventosLocalidades
        [HttpGet]
        public async Task<ActionResult<IEnumerable<EventoLocalidadDto>>> GetEventosLocalidades()
        {
            return Ok(await _eventoLocalidadService.ObtenerEventosLocalidadesAsync());
        }

        [HttpGet("{id}")] 
        public async Task<ActionResult<EventoLocalidadDto>> GetEventoLocalidad (int id)
        {
            var eventoLocalidad = await _eventoLocalidadService.ObtenerEventoLocalidadPorIdAsync(id);
            return eventoLocalidad == null ? NotFound() : Ok(eventoLocalidad);
        }
    }
}