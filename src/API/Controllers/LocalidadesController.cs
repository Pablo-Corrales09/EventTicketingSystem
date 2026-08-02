using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class LocalidadesController : ControllerBase
    {
        // Inyección de dependencias del servicio de localidades para poder utilizar sus métodos en el controlador.
        private readonly LocalidadService _localidadService;

        // Constructor para inyectar el servicio de localidades en el controlador.
        public LocalidadesController(LocalidadService localidadService)
        {
            _localidadService = localidadService;
        }

        // GET: api/Localidades
        // Endpoint para obtener todas las localidades.
        [HttpGet]
        public async Task<ActionResult<IEnumerable<LocalidadDto>>> GetLocalidades()
        {
            return Ok(await _localidadService.ObtenerLocalidadesAsync());
        }
    }
}