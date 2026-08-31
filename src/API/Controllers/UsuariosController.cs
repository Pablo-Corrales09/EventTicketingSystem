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
            
            return CreatedAtAction(nameof(GetUsuario), new { id = usuarioCreado.IdUsuario }, usuarioCreado);
        }

        [HttpGet("roles")]
        public async Task<ActionResult<IEnumerable<RoleDto>>> GetRoles()
        {
            return Ok(await _userService.ObtenerRolesAsync());
        }

        [HttpPut("actualizar/{id}")]
        public async Task<ActionResult<UsuarioDto>> ActualizarUsuario(int id, [FromBody] UsuarioActualizarDto request)
        {
            var usuarioActualizado = await _userService.ActualizarUsuarioAsync(id, request);
            return usuarioActualizado == null ? NotFound() : Ok(usuarioActualizado);
        }

        [HttpPut("desactivar/{id}")]
        public async Task<ActionResult<UsuarioDto>> DesactivarUsuario(int id)
        {
            var usuarioDesactivado = await _userService.DesactivarUsuarioAsync(id);
            return usuarioDesactivado == null ? NotFound() : Ok(usuarioDesactivado);
        }

        [HttpPut("activar/{id}")]
        public async Task<ActionResult<UsuarioDto>> ActivarUsuario(int id)
        {
            var usuarioActivado = await _userService.ActivarUsuarioAsync(id);
            return usuarioActivado == null ? NotFound() : Ok(usuarioActivado);
        }
    }
}