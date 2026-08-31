namespace API.Tests.Helpers;

// Fábricas de entidades para sembrar datos controlados en cada prueba.
public static class DatosPrueba
{
    public const string ContrasenaValida = "Clave123!";

    public static MedioPago CrearMedioPago(int id = 1, string nombre = "Tarjeta de crédito") =>
        new() { IdMedioPago = id, NombreMedioPago = nombre };

    public static Role CrearRole(int id = 1, string nombre = "Cliente") =>
        new() { IdRole = id, NombreRole = nombre };

    public static Usuario CrearUsuario(
        int id = 1,
        string nombre = "Ana",
        string apellido = "Pérez",
        string correo = "ana@correo.com",
        string contrasena = ContrasenaValida,
        Role? role = null)
    {
        role ??= CrearRole();
        return new Usuario
        {
            IdUsuario = id,
            Nombre = nombre,
            Apellido = apellido,
            Correo = correo,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(contrasena),
            IdRole = role.IdRole,
            IdRoleNavigation = role
        };
    }

    public static SedeEvento CrearSedeEvento(int id = 1, string nombre = "Coliseo Central") =>
        new() { IdSedeEvento = id, NombreSedeEvento = nombre, Ubicacion = "Av. Principal 123" };

    public static Localidad CrearLocalidad(int id = 1, int idSede = 1, string nombre = "VIP") =>
        new() { IdLocalidad = id, NombreLocalidad = nombre, IdSedeEvento = idSede };

    public static Evento CrearEvento(
        int id = 1,
        string nombre = "Concierto de Rock",
        DateTime? fecha = null,
        int? idSede = 1) =>
        new()
        {
            IdEvento = id,
            NombreEvento = nombre,
            FechaEvento = fecha ?? DateTime.Today.AddMonths(1),
            HoraEvento = new TimeOnly(20, 0),
            IdSede = idSede
        };

    public static EventoLocalidad CrearEventoLocalidad(
        int id = 1,
        int idEvento = 1,
        int idLocalidad = 1,
        decimal precio = 100m,
        int capacidadDisponible = 50) =>
        new()
        {
            IdEventoLocalidad = id,
            IdEvento = idEvento,
            IdLocalidad = idLocalidad,
            Precio = precio,
            CapacidadDisponible = capacidadDisponible
        };
}