namespace Web.Models;

public class LoginViewModel
{
    public string Correo { get; set; } = string.Empty;

    public string Contrasena { get; set; } = string.Empty;

    public bool Recordarme { get; set; }
}
