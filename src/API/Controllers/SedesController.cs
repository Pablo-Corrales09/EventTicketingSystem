using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class SedesController : ControllerBase
    {
        private readonly SedeEventoService _sedeEventoService;

        public SedesController(SedeEventoService sedeEventoService)
        {
            _sedeEventoService = sedeEventoService;
        }

        // GET: api/Sedes
        // Endpoint para obtener todas las sedes de eventos.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<SedeUbicacionSimpleDto>>> GetSedes()
        {
            return Ok(await _sedeEventoService.ObtenerTodasAsync());
        }
    }
}
