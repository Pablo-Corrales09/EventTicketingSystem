using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using API.Models;
using API.Services;
using API.Dtos;
using API.Data;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
        private readonly LoginService _loginService;
        private readonly TokenService _tokenService;

        public AuthController(LoginService loginService, TokenService tokenService)
        {
            _loginService = loginService;
            _tokenService = tokenService;
        }
    
    
    //POST: api/Auth/login
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        try
        {
            var usuario = await _loginService.ValidarCredencialesAsync(dto);

            if (usuario == null)
            {
                return Unauthorized(new { mensaje = "Correo o contraseña incorrectos." });
            }

            var token = _tokenService.GenerarToken(usuario);

            return Ok(new 
            {
                token = token,
                mensaje = "¡Inicio de sesión exitoso!"
            });
        }
        catch (Exception ex)
        {
            // Esto te devolverá el error exacto en Postman para saber qué falló
            return StatusCode(500, new { errorDetallado = ex.Message, stackTrace = ex.StackTrace });
        }
    }
}