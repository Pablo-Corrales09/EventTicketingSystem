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
                NombreRole = u.IdRoleNavigation != null ? u.IdRoleNavigation.NombreRole: null
            };
        }

        public UserService(DbDevTicketappContext context){
            _context = context;
        }

        //Metodo que devuelve toda la lista de usuarios.
        public async Task<List<UsuarioDto>> ObtenerTodosAsync()
        {
            return await _context.Usuarios
            .Select(u => MapearUsuarioDto(u))
            .ToListAsync();
        }

        public async Task<UsuarioDto?> ObtenerPorIdAsync(int id)
        {
            return await _context.Usuarios
            .Where(u => u.IdUsuario == id)
            .Select(u => MapearUsuarioDto(u))
            .FirstOrDefaultAsync();
        }

        public async Task<UsuarioDto?> BuscarPorCorreoAsync(string correo)
        {
            return await _context.Usuarios
            .Where (u => u.Correo == correo)
            .Select(u => MapearUsuarioDto(u))
            .FirstOrDefaultAsync();
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
    }

}