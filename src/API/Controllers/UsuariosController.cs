using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using API.Data;
using API.Dtos;
using API.Models;

namespace API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsuariosController : ControllerBase
    {
        private readonly DbDevTicketappContext _context;

        public UsuariosController(DbDevTicketappContext context)
        {
            _context = context;
        }

        //GET: api/usuarios
        [HttpGet]
        public async Task<ActionResult<IEnumerable<UsuarioDto>>> GetUsuarios()
        {
            return await _context.Usuarios
                .Select(u => new UsuarioDto {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    NombreRole = u.IdRoleNavigation != null ? u.IdRoleNavigation.NombreRole : null
                })
                .ToListAsync();
        }

        // GET: api/Usuarios/5
        [HttpGet("{id}")]
        public async Task<ActionResult<UsuarioDto>> GetUsuario(int id)
        {
            var usuarioDto = await _context.Usuarios
                .Where(u => u.IdUsuario == id)
                .Select(u => new UsuarioDto {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    NombreRole = u.IdRoleNavigation != null ? u.IdRoleNavigation.NombreRole : null
                })
                .FirstOrDefaultAsync();

            if (usuarioDto == null) return NotFound();

            return usuarioDto;
        }

        // GET: api/Usuarios/buscarCorreo?correo=...
        [HttpGet("buscarCorreo")]
        public async Task<ActionResult<UsuarioDto>> BuscarPorCorreo([FromQuery] string correo)
        {
            var usuarioDto = await _context.Usuarios
                .Where(u => u.Correo == correo)
                .Select(u => new UsuarioDto {
                    IdUsuario = u.IdUsuario,
                    Nombre = u.Nombre,
                    Apellido = u.Apellido,
                    Correo = u.Correo,
                    Telefono = u.Telefono,
                    NombreRole = u.IdRoleNavigation != null ? u.IdRoleNavigation.NombreRole : null
                })
                .FirstOrDefaultAsync();
            
            if (usuarioDto == null) return NotFound();
            
            return usuarioDto;
        }

    }
}