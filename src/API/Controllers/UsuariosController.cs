using Microsoft.AspNetCore.Mvc;
using API.Dtos;
using API.Services;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly UserService _userService;

        public UsuariosController(UserService userService)
        {
            _userService = userService;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            return Ok(await _userService.ObtenerTodosAsync());
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuario = await _userService.ObtenerPorIdAsync(id);
            return usuario == null ? NotFound() : Ok(usuario);
        }

        [HttpGet("buscarCorreo")]
        public async Task<ActionResult<UsuarioDto>> BuscarPorCorreo([FromQuery] string correo)
        {
            var usuario = await _userService.BuscarPorCorreoAsync(correo);
            return usuario == null ? NotFound() : Ok(usuario);
        }

        [HttpPost("crearUsuario")]
        public async Task<ActionResult<UsuarioDto>> CrearUsuario([FromBody] UsuarioCreacionDto request)
        {
            var usuarioCreado = await _userService.CrearUsuario(
                request.Nombre, 
                request.Apellido, 
                request.Telefono,
                request.Correo, 
                request.Contrasena, 
                request.IdRole
            );

            if (usuarioCreado == null)
            {
                return BadRequest("No se pudo registrar el usuario. Verifica los datos enviados."); 
            }

            return CreatedAtAction(nameof(GetUsuario), new { id = usuarioCreado.IdUsuario }, usuarioCreado);
            }
    }
}