using API.Data;
using API.Dtos;
using API.Models;
using BCrypt.Net;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace API.Services
{
    public class LoginService
    {
        private readonly DbDevTicketappContext _context;

        public LoginService(DbDevTicketappContext context)
        {
            _context = context;
        }

    public async Task<Usuario?> ValidarCredencialesAsync(LoginRequestDto dto)
        {
            var usuario = await _context.Usuarios
                .Include(u => u.IdRoleNavigation)
                .FirstOrDefaultAsync(u => u.Correo == dto.Correo);
                
            if (usuario == null)
            {
                return null;
            }
            bool esValida = BCrypt.Net.BCrypt.Verify(dto.Contrasena ?? string.Empty, usuario.PasswordHash);
            
            if (esValida && usuario.IdRoleNavigation?.NombreRole?.Equals("Inactivo", StringComparison.OrdinalIgnoreCase) == true)
            {
                return null; // El usuario está desactivado, no se permite el acceso.
            }

            if (esValida)
            {
                return usuario; // Retorna el usuario completo si la contraseña es correcta
            }
            
            return null; // Retorna null si la contraseña es incorrecta
        }       
    }
}