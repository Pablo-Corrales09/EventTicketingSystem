using API.Data;
using API.Dtos;
using API.Models;
using Microsoft.EntityFrameworkCore;



namespace API.Services
{
    public class UserService
    {
        private readonly DbDevTicketappContext _context;

        //Metodo para no repetir el codigo del mapeo
        private static UsuarioDto MapearUsuarioDto(Models.Usuario u)
        {
            return new UsuarioDto
            {
                IdUsuario = u.IdUsuario,
                Nombre = u.Nombre,
                Apellido = u.Apellido,
                Correo = u.Correo,
                Telefono = u.Telefono,
                IdRole = u.IdRole,
                NombreRole = u.IdRoleNavigation != null ? u.IdRoleNavigation.NombreRole : null
            };
        }

        public UserService(DbDevTicketappContext context){
            _context = context;
        }

        //Metodo que devuelve toda la lista de usuarios.
        public async Task<List<UsuarioDto>> ObtenerTodosAsync()
        {
            var usuarios = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .ToListAsync();

            return usuarios.Select(u => MapearUsuarioDto(u)).ToList();
        }

        public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            return usuario == null ? null : MapearUsuarioDto(usuario);
        }

        public async Task<UsuarioDto?> BuscarPorCorreoAsync(string correo)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.Correo == correo);

            return usuario == null ? null : MapearUsuarioDto(usuario);
        }


        public async Task<UsuarioDto> CrearUsuario(string nombre, string apellido, string? contacto, string correo, string contrasena, int? id_role)
        {
            var existe = await _context.Usuarios.AnyAsync(u => u.Correo == correo);
            if (existe)
            {
                throw new InvalidOperationException("El correo electronico ya esta registrado");
            }


            string passwordHash = BCrypt.Net.BCrypt.HashPassword(contrasena);
            var nuevoUsuario = new Usuario{
                Nombre = nombre,
                Apellido = apellido,
                Telefono = contacto,
                Correo = correo,
                PasswordHash = passwordHash,
                IdRole = id_role ?? 2                
            };

            _context.Add(nuevoUsuario);
            await _context.SaveChangesAsync();

            var usuarioDto = new UsuarioDto
            {
                IdUsuario = nuevoUsuario.IdUsuario,
                Nombre = nuevoUsuario.Nombre,
                Apellido = nuevoUsuario.Apellido,
                Telefono = nuevoUsuario.Telefono,
                Correo = nuevoUsuario.Correo,
                IdRole = nuevoUsuario.IdRole
            };

            return usuarioDto;
        }

        public async Task<List<RoleDto>> ObtenerRolesAsync()
        {
            return await _context.Roles
                .OrderBy(r => r.NombreRole)
                .Select(r => new RoleDto
                {
                    IdRole = r.IdRole,
                    NombreRole = r.NombreRole
                })
                .ToListAsync();
        }

        public async Task<UsuarioDto?> ActualizarUsuarioAsync(int id, UsuarioActualizarDto dto)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null)
            {
                return null;
            }

            var existeCorreo = await _context.Usuarios
                .AnyAsync(u => u.Correo == dto.Correo && u.IdUsuario != id);
            if (existeCorreo)
            {
                throw new InvalidOperationException("El correo electronico ya esta registrado por otro usuario");
            }

            usuario.Nombre = dto.Nombre;
            usuario.Apellido = dto.Apellido;
            usuario.Correo = dto.Correo;
            usuario.Telefono = dto.Telefono;
            usuario.IdRole = dto.IdRole;

            await _context.SaveChangesAsync();

            var usuarioActualizado = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            return usuarioActualizado == null ? null : MapearUsuarioDto(usuarioActualizado);
        }

        public async Task<UsuarioDto?> DesactivarUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null)
            {
                return null;
            }

            var rolInactivo = await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRole.ToLower() == "inactivo");

            if (rolInactivo == null)
            {
                throw new InvalidOperationException("No existe el rol 'Inactivo' para desactivar el usuario.");
            }

            usuario.IdRole = rolInactivo.IdRole;
            await _context.SaveChangesAsync();

            var usuarioDesactivado = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            return usuarioDesactivado == null ? null : MapearUsuarioDto(usuarioDesactivado);
        }

        public async Task<UsuarioDto?> ActivarUsuarioAsync(int id)
        {
            var usuario = await _context.Usuarios.FirstOrDefaultAsync(u => u.IdUsuario == id);
            if (usuario == null)
            {
                return null;
            }

            var rolUsuario = await _context.Roles
                .FirstOrDefaultAsync(r => r.NombreRole.ToLower() == "cliente");

            usuario.IdRole = rolUsuario?.IdRole ?? 2;
            await _context.SaveChangesAsync();

            var usuarioActivado = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.IdUsuario == id);

            return usuarioActivado == null ? null : MapearUsuarioDto(usuarioActivado);
        }
    }

}